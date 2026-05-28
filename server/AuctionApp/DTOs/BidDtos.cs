namespace AuctionApp.DTOs;

public record PlaceBidRequest(int LotId, decimal Amount);

public record BidResponse(
    int Id,
    int LotId,
    UserBrief Bidder,
    decimal Amount,
    DateTime PlacedAt
);
