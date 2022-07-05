using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal interface IExpression
    {
        IValue Evaluate(Call call);
    }
}