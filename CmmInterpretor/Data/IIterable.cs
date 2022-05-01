using System.Collections.Generic;

namespace CmmInterpretor.Data
{
    public interface IIterable
    {
        IEnumerable<Value> Iterate();
    }
}
