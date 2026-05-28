using System.Security.Claims;
using AuctionApp.Data;
using AuctionApp.DTOs;
using AuctionApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApp.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet("me/lots")]
    public async Task<ActionResult<List<MyLotItem>>> GetMyLots()
    {
        var userId = GetUserId();

        var lots = await db.Lots
            .Include(l => l.Status)
            .Include(l => l.Bids).ThenInclude(b => b.Bidder)
            .Where(l => l.SellerId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        await AutoCloseExpiredAsync(lots);

        return Ok(lots.Select(l =>
        {
            var winnerBid = l.WinnerBidId.HasValue
                ? l.Bids.FirstOrDefault(b => b.Id == l.WinnerBidId)
                : null;
            return new MyLotItem(
                l.Id, l.Title, l.ImagePath, l.CurrentPrice,
                l.Bids.Count, l.Status.Name, l.EndsAt,
                winnerBid?.Bidder?.Username);
        }).ToList());
    }

    [HttpGet("me/bids")]
    public async Task<ActionResult<List<MyBidItem>>> GetMyBids()
    {
        var userId = GetUserId();

        var lotIds = await db.Bids
            .Where(b => b.BidderId == userId)
            .Select(b => b.LotId)
            .Distinct()
            .ToListAsync();

        var lots = await db.Lots
            .Include(l => l.Status)
            .Include(l => l.Bids)
            .Include(l => l.WinnerBid)
            .Where(l => lotIds.Contains(l.Id))
            .ToListAsync();

        await AutoCloseExpiredAsync(lots);

        var result = lots.Select(l =>
        {
            var myTopBid = l.Bids
                .Where(b => b.BidderId == userId)
                .Select(b => b.Amount)
                .Max();

            string bidStatus = l.Status.Name switch
            {
                "Sold" => l.WinnerBid?.BidderId == userId ? "Переміг" : "Програв",
                "Active" => myTopBid >= l.CurrentPrice ? "Лідирую" : "Перебито",
                _ => "Скасовано"
            };

            return new MyBidItem(
                l.Id, l.Title, l.ImagePath,
                myTopBid, l.CurrentPrice,
                l.Status.Name, l.EndsAt, bidStatus);
        }).OrderByDescending(r => r.EndsAt).ToList();

        return Ok(result);
    }

    private async Task AutoCloseExpiredAsync(List<Lot> lots)
    {
        var expired = lots
            .Where(l => l.Status.Name == "Active" && l.EndsAt < DateTime.UtcNow && l.Bids.Any())
            .ToList();

        if (expired.Count == 0) return;

        var soldStatus = await db.LotStatuses.FirstAsync(s => s.Name == "Sold");
        foreach (var lot in expired)
        {
            var winner = lot.Bids.MaxBy(b => b.Amount)!;
            lot.StatusId = soldStatus.Id;
            lot.Status = soldStatus;
            lot.WinnerBidId = winner.Id;
            lot.WinnerBid = winner;
            lot.CurrentPrice = winner.Amount;
        }
        await db.SaveChangesAsync();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue("sub")!);
}
