using CmmInterpretor.Values;

namespace CmmInterpretor.Variables
{
    public class HeapVariable : Variable
    {
        public override Value Value { get; set; }

        public HeapVariable(Value value) => Value = value;

        public override void Destroy()
        {
            foreach (var reference in References)
                reference.Invalidate();

            Value.Destroy();
        }
    }
}
