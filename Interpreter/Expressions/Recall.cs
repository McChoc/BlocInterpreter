using Bloc.Memory;
using Bloc.Pointers;

namespace Bloc.Expressions
{
    internal sealed record Recall : IExpression
    {
        public IPointer Evaluate(Call call) => new VariablePointer(call.Recall!);
    }
}