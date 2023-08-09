using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Expressions;

public interface IExpression
{
    IValue Evaluate(Call call);
}