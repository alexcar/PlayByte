using PlayByte.Domain.Common;

namespace PlayByte.Application.Catalog.Playback;

public static class PlaybackErrors
{
    public static readonly Error RequiresPaidPlan =
        Error.Forbidden(
            "Playback.RequiresPaidPlan",
            "A reproducao de musicas e exclusiva do plano pago. Assine para ouvir.");
}
