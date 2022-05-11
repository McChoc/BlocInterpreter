using CmmInterpretor.Results;
using System.Collections.Generic;

namespace CmmInterpretor.Data
{
    public interface IInvokable
    {
        IResult Invoke(List<Value> values, Call call);
    }
}
