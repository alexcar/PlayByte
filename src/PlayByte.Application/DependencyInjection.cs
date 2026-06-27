using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PlayByte.Application.Abstractions.Behaviors;
using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Ordem importa: Logging (externo) envolve Validation (interno) que envolve o handler.
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxODEzMzYzMjAwIiwiaWF0IjoiMTc4MTg4MjI4OCIsImFjY291bnRfaWQiOiIwMTllZTA3NTRhZjU3NjBmYTY1MWVmOTA5MDM2NWYzNCIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa3ZnN2J5MTE1NXR2cXNnOTRoOHpjNWNqIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.pQA06jsNpSgGvOhzMjOM0WFV7aXDp9a0InVcPK6P7gkVSD5XJ4O7EBO0YlvL73mSeFH1STx_3j3HNkIPh17Uun-LJ7y3Y5Zn_66UB-sZhllQo8yigiIud2FsdXHH8ivKKcv5w94v0uUt0x3BOuqTSVi8PQKVZQ1S2VwFa5gbLlO5LJmq_0YS1VuKuWosCQt-GIwaBqZasZ5SXpZA777vY4savFl2vugNiUhnPDWU1j5iFljGRNfHNPUBUK0DUefiQI_JP2RiLFaVNtkZh7AUttpcdRf8FjvIHLzRJhNt2NixcdXMTDMOi87jdktA5QTxTgiqmXAU4eHBu191mCTMsA";
        });

        // AutoMapper 14.x: extensao no pacote principal.
        // services.AddAutoMapper(assembly);

        // AutoMapper 16.x
        services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxODEzMzYzMjAwIiwiaWF0IjoiMTc4MTg4MjI4OCIsImFjY291bnRfaWQiOiIwMTllZTA3NTRhZjU3NjBmYTY1MWVmOTA5MDM2NWYzNCIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa3ZnN2J5MTE1NXR2cXNnOTRoOHpjNWNqIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.pQA06jsNpSgGvOhzMjOM0WFV7aXDp9a0InVcPK6P7gkVSD5XJ4O7EBO0YlvL73mSeFH1STx_3j3HNkIPh17Uun-LJ7y3Y5Zn_66UB-sZhllQo8yigiIud2FsdXHH8ivKKcv5w94v0uUt0x3BOuqTSVi8PQKVZQ1S2VwFa5gbLlO5LJmq_0YS1VuKuWosCQt-GIwaBqZasZ5SXpZA777vY4savFl2vugNiUhnPDWU1j5iFljGRNfHNPUBUK0DUefiQI_JP2RiLFaVNtkZh7AUttpcdRf8FjvIHLzRJhNt2NixcdXMTDMOi87jdktA5QTxTgiqmXAU4eHBu191mCTMsA";
            cfg.AddMaps(assembly);
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Handlers de eventos de dominio (IDomainEventHandler<T>): registro por reflexao,
        // ja que MediatR nao os enxerga mais (o Domain nao depende de MediatR).
        RegisterDomainEventHandlers(services, assembly);

        return services;
    }

    private static void RegisterDomainEventHandlers(IServiceCollection services, System.Reflection.Assembly assembly)
    {
        var openHandler = typeof(IDomainEventHandler<>);

        foreach (var type in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
        {
            foreach (var @interface in type.GetInterfaces()
                         .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openHandler))
            {
                services.AddScoped(@interface, type);
            }
        }
    }
}
