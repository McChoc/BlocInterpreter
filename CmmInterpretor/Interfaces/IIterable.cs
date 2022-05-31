using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Interfaces
{
    internal interface IIterable
    {
        IEnumerable<Value> Iterate();
    }
}
