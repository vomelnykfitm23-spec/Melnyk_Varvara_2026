namespace AuctionApp.DTOs;

public record MyLotItem(
    int Id,
    string Title,
    string? ImagePath,
    decimal CurrentPrice,
    int BidCount,
    string Status,
    DateTime EndsAt,
    string? WinnerUsername
);

public record MyBidItem(
    int LotId,
    string LotTitle,
    string? LotImagePath,
    decimal MyTopBid,
    decimal CurrentPrice,
    string LotStatus,
    DateTime EndsAt,
    string BidStatus
);
