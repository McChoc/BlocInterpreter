using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    internal class Params : IExpression
    {
        public IValue Evaluate(Call call) => call.Params!;
    }
}
