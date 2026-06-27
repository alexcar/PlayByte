using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Pagination;

namespace PlayByte.Application.Catalog.Queries.ListBands;

public sealed record ListBandsQuery(int Page = 1, int PageSize = 20) : IQuery<PagedResult<BandListItem>>;

public sealed record BandListItem(Guid Id, string Name, string? CoverImageUrl);
