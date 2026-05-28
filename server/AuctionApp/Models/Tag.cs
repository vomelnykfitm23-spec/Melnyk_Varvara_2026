namespace AuctionApp.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<Lot> Lots { get; set; } = [];
}
