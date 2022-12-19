using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Variables
{
    public sealed class StackVariable : Variable
    {
        private readonly Scope _scope;

        internal string Name { get; }

        internal StackVariable(bool mutable, string name, Value value, Scope scope)
            : base(mutable, value)
        {
            _scope = scope;
            Name = name;
        }

        public override void Delete()
        {
            _scope.Remove(Name);

            base.Delete();
        }
    }
}