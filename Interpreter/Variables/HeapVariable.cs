using Bloc.Results;
using Bloc.Values;

namespace Bloc.Variables
{
    public sealed class HeapVariable : Variable
    {
        internal HeapVariable(bool mutable, Value value) : base(value)
        {
            Mutable = mutable;
        }

        internal bool Mutable { get; }

        public override Value Value
        {
            get => _value;
            set => base.Value = Mutable
                ? value
                : throw new Throw("Cannot assign a value to a readonly variable");
        }
    }
}