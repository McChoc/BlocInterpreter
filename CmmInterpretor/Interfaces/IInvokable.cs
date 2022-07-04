using System.Collections.Generic;
using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Interfaces
{
    internal interface IInvokable
    {
        IValue Invoke(List<Value> values, Call call);
    }
}