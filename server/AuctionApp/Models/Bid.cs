namespace AuctionApp.Models;

public class Bid
{
    public int Id { get; set; }
    public int LotId { get; set; }
    public int BidderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;

    public Lot Lot { get; set; } = null!;
    public User Bidder { get; set; } = null!;
}
