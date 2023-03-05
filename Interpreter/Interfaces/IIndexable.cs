using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Interfaces;

internal interface IIndexable
{
    IValue Index(Value value, Call call);
}