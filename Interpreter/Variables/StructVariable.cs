using Bloc.Values;

namespace Bloc.Variables
{
    internal class StructVariable : Variable
    {
        private readonly Struct _parent;

        internal StructVariable(string key, Value value, Struct parent) : base(value)
        {
            _parent = parent;
            Name = key;
        }

        public string Name { get; }

        public override void Delete()
        {
            _parent.Values.Remove(Name);

            base.Delete();
        }
    }
}