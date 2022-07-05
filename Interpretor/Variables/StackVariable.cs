using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Variables
{
    internal class StackVariable : Variable
    {
        private readonly Scope _scope;

        internal StackVariable(Value value, string name, Scope scope)
        {
            Value = value;
            Name = name;
            _scope = scope;
        }

        public override Value Value { get; set; }
        internal string Name { get; }

        public override void Destroy()
        {
            _scope.Variables.Remove(Name);

            foreach (var reference in References)
                reference.Invalidate();

            Value.Destroy();
        }
    }
}