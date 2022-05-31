using CmmInterpretor.Memory;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;

namespace CmmInterpretor.Expressions
{
    internal class Identifier : IExpression
    {
        private readonly string _name;

        internal Identifier(string name) => _name = name;

        public IValue Evaluate(Call call)
        {
            if (call.TryGet(_name, out Variable? variable))
                return variable!;
            
            return new UndefinedVariable(_name);
        }
    }
}
