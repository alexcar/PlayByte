using System.Data;
using Dapper;

namespace PlayByte.Infrastructure.Persistence;

/// <summary>
/// Ensina o Dapper a converter <see cref="DateOnly"/> &lt;-&gt; coluna <c>date</c> do SQL Server,
/// tanto como parametro quanto na leitura. O Microsoft.Data.SqlClient nao aceita DateOnly
/// diretamente como valor de parametro, entao convertemos para/da DateTime.
/// </summary>
internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
}
