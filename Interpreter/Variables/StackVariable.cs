using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Variables
{
    public sealed class StackVariable : Variable
    {
        private readonly Scope _scope;

        internal StackVariable(bool mutable, string name, Value value, Scope scope) : base(value)
        {
            Mutable = mutable;
            Name = name;
            _scope = scope;
        }

        internal bool Mutable { get; }

        internal string Name { get; }

        public override Value Value
        {
            get => _value;
            set => base.Value = Mutable
                ? value
                : throw new Throw("Cannot assign a value to a readonly variable");
        }

        public override void Delete()
        {
            _scope.Variables.Remove(Name);

            base.Delete();
        }
    }
}