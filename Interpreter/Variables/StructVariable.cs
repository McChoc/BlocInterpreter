using Bloc.Values;

namespace Bloc.Variables
{
    internal sealed class StructVariable : Variable
    {
        private readonly Struct _parent;

        internal StructVariable(string name, Value value, Struct parent) : base(value)
        {
            Name = name;
            _parent = parent;
        }

        public string Name { get; }

        public override void Delete()
        {
            _parent.Variables.Remove(Name);

            base.Delete();
        }
    }
}