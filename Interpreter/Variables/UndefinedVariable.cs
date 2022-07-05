using Bloc.Results;
using Bloc.Values;

namespace Bloc.Variables
{
    internal class UndefinedVariable : Variable
    {
        internal UndefinedVariable(string name) => Name = name;

        public override Value Value
        {
            get => throw new Throw($"Variable {Name} was not defined in scope.");
            set => throw new Throw($"Variable {Name} was not defined in scope.");
        }

        internal string Name { get; }

        public override void Destroy() { }
    }
}