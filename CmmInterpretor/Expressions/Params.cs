using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    public class Params : IExpression
    {
        public IValue Evaluate(Call call) => call.Params!;
    }
}
