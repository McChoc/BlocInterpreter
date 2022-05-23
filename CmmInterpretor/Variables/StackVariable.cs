using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Variables
{
    public class StackVariable : Variable
    {
        private readonly Scope _scope;

        public override Value Value { get; set; }
        public string Name { get; }

        public StackVariable(Value value, string name, Scope scope)
        {
            Value = value;
            Name = name;
            _scope = scope;
        }

        public override void Destroy()
        {
            _scope.Variables.Remove(Name);

            foreach (var reference in References)
                reference.Invalidate();

            Value.Destroy();
        }
    }
}
