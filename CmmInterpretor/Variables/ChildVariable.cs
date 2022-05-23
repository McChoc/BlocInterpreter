using CmmInterpretor.Values;

namespace CmmInterpretor.Variables
{
    public class ChildVariable : Variable
    {
        //private readonly object? _accessor;
        //private readonly Value _parent;

        public override Value Value { get; set; }

        public ChildVariable(Value value, object? accessor, Value parent)
        {
            Value = value;
            //_accessor = accessor;
            //_parent = parent;
        }

        public override void Destroy()
        {
            //if (_accessor is not null)
            //    _parent.Remove(_accessor);

            foreach (var reference in References)
                reference.Invalidate();

            Value.Destroy();
        }
    }
}
