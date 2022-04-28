using System;

namespace CmmInterpretor.Exceptions
{
    public class SyntaxError : Exception
    {
        public SyntaxError(string message) : base(message) { }
    }
}
