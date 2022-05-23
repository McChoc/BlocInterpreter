using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    public interface IExpression
    {
        IValue Evaluate(Call call);
    }
}
