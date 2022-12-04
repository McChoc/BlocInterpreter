using Bloc.Values;

namespace Bloc.Variables
{
    internal sealed class ArrayVariable : Variable
    {
        private readonly Array _parent;

        internal ArrayVariable(Value value, Array parent) : base(value)
        {
            _parent = parent;
        }

        public override void Delete()
        {
            _parent.Variables.Remove(this);

            base.Delete();
        }
    }
}