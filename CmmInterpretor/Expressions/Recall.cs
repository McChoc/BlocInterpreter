using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    public class Recall : IExpression
    {
        public IValue Evaluate(Call call) => call.Recall!;
    }
}
