using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal class Params : IExpression
    {
        public IValue Evaluate(Call call) => call.Params!;
    }
}