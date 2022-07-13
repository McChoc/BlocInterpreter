using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Pointers
{
    internal abstract class Pointer : IPointer
    {
        Value IPointer.Value => Get();

        internal abstract Pointer Define(Value value, Call call);

        internal abstract Value Get();

        internal abstract Value Set(Value value);

        internal abstract Value Delete();

        internal abstract void Invalidate();

        internal abstract bool Equals(Pointer other);
    }
}