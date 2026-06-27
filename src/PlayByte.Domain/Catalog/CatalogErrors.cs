using PlayByte.Domain.Common;

namespace PlayByte.Domain.Catalog;

public static class CatalogErrors
{
    public static readonly Error BandNameRequired =
        Error.Validation("Catalog.BandNameRequired", "O nome da banda e obrigatorio.");
    public static readonly Error AlbumTitleRequired =
        Error.Validation("Catalog.AlbumTitleRequired", "O titulo do album e obrigatorio.");
    public static readonly Error TrackTitleRequired =
        Error.Validation("Catalog.TrackTitleRequired", "O titulo da faixa e obrigatorio.");
    public static readonly Error InvalidReleaseYear =
        Error.Validation("Catalog.InvalidReleaseYear", "Ano de lancamento invalido.");
    public static readonly Error InvalidDuration =
        Error.Validation("Catalog.InvalidDuration", "A duracao da faixa deve ser maior que zero.");
    public static readonly Error AlbumNotFound =
        Error.NotFound("Catalog.AlbumNotFound", "Album nao encontrado nesta banda.");

    public static Error BandNotFound(BandId id) =>
        Error.NotFound("Catalog.BandNotFound", $"Banda {id} nao encontrada.");
    public static Error TrackNotFound(TrackId id) =>
        Error.NotFound("Catalog.TrackNotFound", $"Faixa {id} nao encontrada.");
}
