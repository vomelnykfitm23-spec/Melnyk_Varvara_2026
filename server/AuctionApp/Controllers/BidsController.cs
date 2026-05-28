using System.Security.Claims;
using AuctionApp.Data;
using AuctionApp.DTOs;
using AuctionApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApp.Controllers;

[ApiController]
[Route("api/bids")]
public class BidsController(AppDbContext db) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidResponse>> Place(PlaceBidRequest req)
    {
        var bidderId = GetUserId();

        var lot = await db.Lots
            .Include(l => l.Status)
            .FirstOrDefaultAsync(l => l.Id == req.LotId);

        if (lot is null)
            return NotFound(new { message = "Lot not found" });

        if (lot.Status.Name != "Active")
            return BadRequest(new { message = "Lot is not active" });

        if (lot.EndsAt <= DateTime.UtcNow)
            return BadRequest(new { message = "Auction has already ended" });

        if (lot.SellerId == bidderId)
            return BadRequest(new { message = "You cannot bid on your own lot" });

        if (req.Amount <= lot.CurrentPrice)
            return BadRequest(new { message = $"Ставка має бути більше ₴{lot.CurrentPrice:F2}" });

        var bid = new Bid
        {
            LotId = lot.Id,
            BidderId = bidderId,
            Amount = req.Amount,
        };

        db.Bids.Add(bid);
        lot.CurrentPrice = req.Amount;
        await db.SaveChangesAsync();

        var bidder = await db.Users.FindAsync(bidderId);

        return CreatedAtAction(nameof(Place), new BidResponse(
            bid.Id, bid.LotId,
            new UserBrief(bidderId, bidder!.Username),
            bid.Amount, bid.PlacedAt
        ));
    }

    private int GetUserId() => int.Parse(User.FindFirstValue("sub")!);
}
