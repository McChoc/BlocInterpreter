using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed record Params : IExpression
    {
        public IValue Evaluate(Call call) => new VariablePointer(call.Params!);
    }
}