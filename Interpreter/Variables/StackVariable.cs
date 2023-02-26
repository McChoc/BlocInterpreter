using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Variables
{
    public sealed class StackVariable : Variable
    {
        private readonly VariableCollection _collection;

        internal string Name { get; }

        internal StackVariable(bool mutable, string name, Value value, VariableCollection collection)
            : base(mutable, value)
        {
            _collection = collection;
            Name = name;
        }

        public override void Delete()
        {
            _collection.Remove(Name);

            base.Delete();
        }
    }
}