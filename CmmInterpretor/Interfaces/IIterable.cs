using System.Collections.Generic;
using CmmInterpretor.Values;

namespace CmmInterpretor.Interfaces
{
    internal interface IIterable
    {
        IEnumerable<Value> Iterate();
    }
}