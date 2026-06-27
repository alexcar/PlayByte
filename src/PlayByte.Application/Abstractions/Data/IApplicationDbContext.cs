namespace PlayByte.Application.Abstractions.Data;

/// <summary>
/// Porta do lado de escrita. O proprio DbContext do EF Core ja implementa o padrao
/// Unit of Work internamente (rastreia as mudancas e as confirma numa unica transacao
/// em SaveChanges), por isso NAO ha um Unit of Work artesanal: esta interface apenas
/// expoe a funcionalidade nativa do DbContext para a camada de Application, sem acopla-la
/// ao EF Core. A implementacao e' o ApplicationDbContext.
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
