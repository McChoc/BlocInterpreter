using CmmInterpretor.Memory;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Interfaces
{
    internal interface IInvokable
    {
        IValue Invoke(List<Value> values, Call call);
    }
}
