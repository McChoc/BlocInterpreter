using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Variables
{
    internal class UndefinedVariable : Variable
    {
        public override Value Value
        {
            get => throw new Throw($"Variable {Name} was not defined in scope.");
            set => throw new Throw($"Variable {Name} was not defined in scope.");
        }

        internal string Name { get; }

        internal UndefinedVariable(string name) => Name = name;

        public override void Destroy() { }
    }
}
