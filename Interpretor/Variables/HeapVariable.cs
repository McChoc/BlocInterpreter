using Bloc.Values;

namespace Bloc.Variables
{
    internal class HeapVariable : Variable
    {
        internal HeapVariable(Value value) => Value = value;

        public override Value Value { get; set; }

        public override void Destroy()
        {
            foreach (var reference in References)
                reference.Invalidate();

            Value.Destroy();
        }
    }
}