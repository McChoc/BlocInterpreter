using CmmInterpretor.Data;

namespace CmmInterpretor.Variables
{
    public class StackVariable : Variable
    {
        private readonly string _name;
        private readonly Scope _scope;

        public string Name => _name;

        public StackVariable(Value value, string name, Scope scope) : base(value)
        {
            _name = name;
            _scope = scope;
        }

        public override void Destroy()
        {
            _scope.Variables.Remove(_name);

            foreach (var reference in References)
                reference.Invalidate();

            Value.Destroy();
        }
    }
}
