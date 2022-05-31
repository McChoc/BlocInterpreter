using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    internal interface IExpression
    {
        IValue Evaluate(Call call);
    }
}
