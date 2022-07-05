using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal class Recall : IExpression
    {
        public IValue Evaluate(Call call) => call.Recall!;
    }
}