using Bloc.Values;

namespace Bloc.Variables
{
    internal class ChildVariable : Variable
    {
        //private readonly object? _accessor;
        //private readonly Value _parent;

        internal ChildVariable(Value value, object? accessor, Value parent)
        {
            Value = value;
            //_accessor = accessor;
            //_parent = parent;
        }

        public override Value Value { get; set; }

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