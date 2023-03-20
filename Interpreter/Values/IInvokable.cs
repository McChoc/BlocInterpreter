using System.Collections.Generic;
using Bloc.Memory;

namespace Bloc.Values;

internal interface IInvokable
{
    Value Invoke(List<Value> args, Dictionary<string, Value> kwargs, Call call);
}