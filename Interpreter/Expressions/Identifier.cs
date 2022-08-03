using Bloc.Memory;
using Bloc.Pointers;

namespace Bloc.Expressions
{
    internal class Identifier : IExpression
    {
        internal string Name { get; }

        internal Identifier(string name) => Name = name;

        public IPointer Evaluate(Call call) => call.Get(Name);
    }
}