using Bloc.Memory;
using Bloc.Pointers;

namespace Bloc.Expressions
{
    internal class Recall : IExpression
    {
        public IPointer Evaluate(Call call) => new VariablePointer(call.Recall!);
    }
}