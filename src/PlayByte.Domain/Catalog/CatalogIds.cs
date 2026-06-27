namespace PlayByte.Domain.Catalog;

public readonly record struct BandId(Guid Value)
{
    public static BandId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public readonly record struct AlbumId(Guid Value)
{
    public static AlbumId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public readonly record struct TrackId(Guid Value)
{
    public static TrackId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}
