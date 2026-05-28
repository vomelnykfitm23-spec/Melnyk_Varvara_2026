namespace AuctionApp.Models;

public class Lot
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public int SellerId { get; set; }
    public int StatusId { get; set; }
    public int? WinnerBidId { get; set; }
    public DateTime EndsAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Seller { get; set; } = null!;
    public LotStatus Status { get; set; } = null!;
    public Bid? WinnerBid { get; set; }
    public ICollection<Bid> Bids { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
}
