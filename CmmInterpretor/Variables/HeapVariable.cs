using CmmInterpretor.Data;

namespace CmmInterpretor.Variables
{
    public class HeapVariable : Variable
    {
        public HeapVariable(Value value) : base(value) { }

        public override void Destroy()
        {
            foreach (var reference in References)
                reference.Invalidate();

            Value.Destroy();
        }
    }
}
