using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Interfaces
{
    public interface IIndexable
    {
        IValue Index(Value value, Call call);
    }
}
