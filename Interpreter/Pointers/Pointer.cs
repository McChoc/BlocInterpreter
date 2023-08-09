using Bloc.Values.Core;

namespace Bloc.Pointers;

public abstract class Pointer : IValue
{
    Value IValue.Value => Get();

    public abstract Value Get();
    public abstract Value Set(Value value);
    public abstract Value Delete();
}