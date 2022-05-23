using System;

namespace CmmInterpretor.Utils.Exceptions
{
    public class SyntaxError : Exception
    {
        public SyntaxError(string message) : base(message) { }
    }
}
