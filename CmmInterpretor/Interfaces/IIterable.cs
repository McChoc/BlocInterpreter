using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Interfaces
{
    public interface IIterable
    {
        IEnumerable<Value> Iterate();
    }
}
