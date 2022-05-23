using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Variables
{
    public class UndefinedVariable : Variable
    {
        public override Value Value
        {
            get => throw new Throw($"Variable {Name} was not defined in scope.");
            set => throw new Throw($"Variable {Name} was not defined in scope.");
        }

        public string Name { get; }

        public UndefinedVariable(string name) => Name = name;

        public override void Destroy() { }
    }
}
