using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Interfaces
{
    internal interface IIndexable
    {
        IValue Index(Value value, Call call);
    }
}