using Bloc.Memory;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Pointers
{
    internal abstract class Pointer : IVariable
    {
        Value IValue.Value => Get();

        internal abstract Pointer Define(bool mutable, Value value, Call call);

        internal abstract Value Get();

        internal abstract Value Set(Value value);

        internal abstract Value Delete();

        internal abstract void Invalidate();

        internal abstract bool Equals(Pointer other);
    }
}