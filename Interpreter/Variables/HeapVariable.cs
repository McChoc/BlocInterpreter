using Bloc.Values.Core;

namespace Bloc.Variables;

public sealed class HeapVariable : Variable
{
    internal HeapVariable(bool mutable, Value value)
        : base(mutable, value) { }
}