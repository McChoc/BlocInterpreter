using System.Collections.Generic;
using Bloc.Values;

namespace Bloc.Interfaces
{
    internal interface IIterable
    {
        IEnumerable<Value> Iterate();
    }
}