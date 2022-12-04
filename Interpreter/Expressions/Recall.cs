using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed record Recall : IExpression
    {
        public IValue Evaluate(Call call) => new VariablePointer(call.Recall!);
    }
}