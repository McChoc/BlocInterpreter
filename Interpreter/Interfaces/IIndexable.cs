using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Interfaces
{
    internal interface IIndexable
    {
        IPointer Index(Value value, Call call);
    }
}