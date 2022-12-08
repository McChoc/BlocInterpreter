using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Interfaces
{
    internal interface IInvokable
    {
        Value Invoke(List<Value> args, Dictionary<string, Value> kwargs, Call call);
    }
}