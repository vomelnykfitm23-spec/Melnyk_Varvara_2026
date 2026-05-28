using System.Security.Claims;
using AuctionApp.Data;
using AuctionApp.DTOs;
using AuctionApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApp.Controllers;

[ApiController]
[Route("api/lots")]
public class LotsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<PagedResult<LotSummary>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int? tagId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.Lots
            .Include(l => l.Seller)
            .Include(l => l.Status)
            .Include(l => l.Tags)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l => l.Title.ToLower().Contains(search.ToLower()));

        if (tagId.HasValue)
            query = query.Where(l => l.Tags.Any(t => t.Id == tagId.Value));

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => ToSummary(l))
            .ToListAsync();

        return new PagedResult<LotSummary>(items, total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LotDetail>> GetById(int id)
    {
        var lot = await db.Lots
            .Include(l => l.Seller)
            .Include(l => l.Status)
            .Include(l => l.Tags)
            .Include(l => l.Bids)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lot is null) return NotFound(new { message = "Lot not found" });

        if (lot.Status.Name == "Active" && lot.EndsAt < DateTime.UtcNow && lot.Bids.Any())
        {
            var soldStatus = await db.LotStatuses.FirstAsync(s => s.Name == "Sold");
            var winner = lot.Bids.MaxBy(b => b.Amount)!;
            lot.StatusId = soldStatus.Id;
            lot.Status = soldStatus;
            lot.WinnerBidId = winner.Id;
            lot.CurrentPrice = winner.Amount;
            await db.SaveChangesAsync();
        }

        string? sellerEmail = lot.Status.Name == "Sold" ? lot.Seller.Email : null;

        return Ok(new LotDetail(
            lot.Id, lot.Title, lot.Description, lot.ImagePath,
            lot.StartingPrice, lot.CurrentPrice,
            new UserBrief(lot.Seller.Id, lot.Seller.Username),
            lot.Status.Name,
            lot.WinnerBidId,
            lot.Tags.Select(t => new TagBrief(t.Id, t.Name)).ToList(),
            lot.EndsAt, lot.CreatedAt,
            sellerEmail
        ));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<LotDetail>> Create(CreateLotRequest req)
    {
        var sellerId = GetUserId();
        var activeStatus = await db.LotStatuses.FirstAsync(s => s.Name == "Active");

        var lot = new Lot
        {
            Title = req.Title,
            Description = req.Description,
            ImagePath = req.ImagePath,
            StartingPrice = req.StartingPrice,
            CurrentPrice = req.StartingPrice,
            SellerId = sellerId,
            StatusId = activeStatus.Id,
            EndsAt = req.EndsAt.ToUniversalTime(),
        };

        if (req.TagIds.Count > 0)
        {
            var tags = await db.Tags.Where(t => req.TagIds.Contains(t.Id)).ToListAsync();
            lot.Tags = tags;
        }

        db.Lots.Add(lot);
        await db.SaveChangesAsync();

        await db.Entry(lot).Reference(l => l.Seller).LoadAsync();
        await db.Entry(lot).Reference(l => l.Status).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = lot.Id }, new LotDetail(
            lot.Id, lot.Title, lot.Description, lot.ImagePath,
            lot.StartingPrice, lot.CurrentPrice,
            new UserBrief(lot.Seller.Id, lot.Seller.Username),
            lot.Status.Name,
            null,
            lot.Tags.Select(t => new TagBrief(t.Id, t.Name)).ToList(),
            lot.EndsAt, lot.CreatedAt,
            null
        ));
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = GetUserId();

        var lot = await db.Lots
            .Include(l => l.Status)
            .Include(l => l.Bids)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lot is null) return NotFound(new { message = "Lot not found" });
        if (lot.SellerId != userId) return Forbid();
        if (lot.Status.Name != "Active") return BadRequest(new { message = "Only active lots can be cancelled" });
        if (lot.Bids.Count > 0) return BadRequest(new { message = "Cannot cancel a lot that already has bids" });

        var cancelled = await db.LotStatuses.FirstAsync(s => s.Name == "Cancelled");
        lot.StatusId = cancelled.Id;
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:int}/bids")]
    public async Task<ActionResult<List<BidResponse>>> GetBids(int id)
    {
        var exists = await db.Lots.AnyAsync(l => l.Id == id);
        if (!exists) return NotFound(new { message = "Lot not found" });

        var bids = await db.Bids
            .Include(b => b.Bidder)
            .Where(b => b.LotId == id)
            .OrderBy(b => b.PlacedAt)
            .Select(b => new BidResponse(
                b.Id, b.LotId,
                new UserBrief(b.Bidder.Id, b.Bidder.Username),
                b.Amount, b.PlacedAt))
            .ToListAsync();

        return Ok(bids);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue("sub")!);

    private static LotSummary ToSummary(Lot l) => new(
        l.Id, l.Title, l.ImagePath, l.StartingPrice, l.CurrentPrice,
        new UserBrief(l.Seller.Id, l.Seller.Username),
        l.Status.Name,
        l.Tags.Select(t => new TagBrief(t.Id, t.Name)).ToList(),
        l.EndsAt, l.CreatedAt
    );
}
