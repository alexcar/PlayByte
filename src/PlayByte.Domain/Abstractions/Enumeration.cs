namespace PlayByte.Domain.Abstractions;

/// <summary>Enumeration Class: conjunto fechado de valores nomeados com comportamento.</summary>
public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    public int Id { get; }
    public string Name { get; }

    protected Enumeration(int id, string name) => (Id, Name) = (id, name);

    public bool Equals(Enumeration<TEnum>? other) =>
        other is not null && GetType() == other.GetType() && Id == other.Id;

    public override bool Equals(object? obj) => obj is Enumeration<TEnum> e && Equals(e);
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
    public override string ToString() => Name;
}
