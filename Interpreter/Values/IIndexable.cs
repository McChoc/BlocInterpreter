using Bloc.Memory;

namespace Bloc.Values;

internal interface IIndexable
{
    IValue Index(Value value, Call call);
}