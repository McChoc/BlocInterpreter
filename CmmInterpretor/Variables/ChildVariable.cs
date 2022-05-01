using CmmInterpretor.Data;

namespace CmmInterpretor.Variables
{
    public class ChildVariable : Variable
    {
        private readonly object _accessor;
        private readonly Value _parent;

        public ChildVariable(Value value, object accessor, Value parent) : base(value)
        {
            _accessor = accessor;
            _parent = parent;
        }

        public override void Destroy()
        {
            _parent.Remove(_accessor);

            foreach (var reference in References)
                reference.Invalidate();

            Value.Destroy();
        }
    }
}
