using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions;

public interface IExpression
{
    IValue Evaluate(Call call);
}