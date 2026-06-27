namespace PlayByte.Domain.Catalog;

public interface IBandRepository
{
    Task<Band?> GetByIdAsync(BandId id, CancellationToken ct = default);
    void Add(Band band);
}
