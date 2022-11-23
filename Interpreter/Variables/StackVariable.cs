using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Variables
{
    public sealed class StackVariable : Variable
    {
        private readonly Scope _scope;

        internal StackVariable(string name, Value value, Scope scope) : base(value)
        {
            _scope = scope;
            Name = name;
        }

        internal string Name { get; }

        public override void Delete()
        {
            _scope.Variables.Remove(Name);

            base.Delete();
        }
    }
}