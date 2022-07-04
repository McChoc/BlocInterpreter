using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Interfaces
{
    internal interface IIndexable
    {
        IValue Index(Value value, Call call);
    }
}