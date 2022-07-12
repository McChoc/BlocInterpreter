using Bloc.Memory;
using Bloc.Pointers;

namespace Bloc.Expressions
{
    public interface IExpression
    {
        IPointer Evaluate(Call call);
    }
}