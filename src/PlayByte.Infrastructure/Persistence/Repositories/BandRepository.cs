using Microsoft.EntityFrameworkCore;
using PlayByte.Domain.Catalog;

namespace PlayByte.Infrastructure.Persistence.Repositories;

internal sealed class BandRepository(ApplicationDbContext context) : IBandRepository
{
    public async Task<Band?> GetByIdAsync(BandId id, CancellationToken ct = default)
        => await context.Bands
            .Include(b => b.Albums)
            .ThenInclude(a => a.Tracks)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public void Add(Band band) => context.Bands.Add(band);
}
