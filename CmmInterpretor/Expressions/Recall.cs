using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    internal class Recall : IExpression
    {
        public IValue Evaluate(Call call) => call.Recall!;
    }
}