using Bloc.Values;

namespace Bloc.Pointers;

internal abstract class Pointer : IValue
{
    Value IValue.Value => Get();

    internal abstract Value Get();
    internal abstract Value Set(Value value);
    internal abstract Value Delete();

    internal abstract void Invalidate();

    public abstract override int GetHashCode();
    public abstract override bool Equals(object other);

    public static bool operator ==(Pointer left, Pointer right) => Equals(left, right);
    public static bool operator !=(Pointer left, Pointer right) => !Equals(left, right);
}