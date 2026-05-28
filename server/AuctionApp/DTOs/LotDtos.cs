namespace AuctionApp.DTOs;

public record UserBrief(int Id, string Username);
public record TagBrief(int Id, string Name);

public record LotSummary(
    int Id,
    string Title,
    string? ImagePath,
    decimal StartingPrice,
    decimal CurrentPrice,
    UserBrief Seller,
    string Status,
    List<TagBrief> Tags,
    DateTime EndsAt,
    DateTime CreatedAt
);

public record LotDetail(
    int Id,
    string Title,
    string? Description,
    string? ImagePath,
    decimal StartingPrice,
    decimal CurrentPrice,
    UserBrief Seller,
    string Status,
    int? WinnerBidId,
    List<TagBrief> Tags,
    DateTime EndsAt,
    DateTime CreatedAt,
    string? SellerEmail
);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);

public record CreateLotRequest(
    string Title,
    string? Description,
    decimal StartingPrice,
    List<int> TagIds,
    DateTime EndsAt,
    string? ImagePath
);
