using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Interfaces
{
    internal interface IInvokable
    {
        IValue Invoke(List<Value> values, Call call);
    }
}