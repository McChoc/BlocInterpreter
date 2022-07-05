using Bloc.Memory;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Expressions
{
    internal class Identifier : IExpression
    {
        private readonly string _name;

        internal Identifier(string name)
        {
            _name = name;
        }

        public IValue Evaluate(Call call)
        {
            if (call.TryGet(_name, out var variable))
                return variable!;

            return new UndefinedVariable(_name);
        }
    }
}