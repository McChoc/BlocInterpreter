using Bloc.Values;

namespace Bloc.Variables
{
    public sealed class HeapVariable : Variable
    {
        internal HeapVariable(Value value) : base(value) { }
    }
}