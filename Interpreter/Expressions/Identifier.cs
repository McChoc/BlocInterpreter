using Bloc.Memory;
using Bloc.Pointers;

namespace Bloc.Expressions
{
    internal class Identifier : IExpression
    {
        private readonly string _name;

        internal Identifier(string name) => _name = name;

        public IPointer Evaluate(Call call) => call.Get(_name);
    }
}