using Bloc.Values;

namespace Bloc.Pointers;

internal abstract class Pointer : IValue
{
    Value IValue.Value => Get();

    internal abstract Value Get();
    internal abstract Value Set(Value value);
    internal abstract Value Delete();
}