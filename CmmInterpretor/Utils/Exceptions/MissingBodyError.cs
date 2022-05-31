namespace CmmInterpretor.Utils.Exceptions
{
    public class MissingBodyError : SyntaxError
    {
        private readonly bool _fatal;

        public override bool Fatal => _fatal;

        public MissingBodyError(int start, int end, bool fatal = true) : base(start, end, "Missing body")
        {
            _fatal = fatal;
        }
    }
}
