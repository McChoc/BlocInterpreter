namespace Bloc.Utils.Exceptions
{
    public class MissingBodyError : SyntaxError
    {
        public MissingBodyError(int start, int end, bool fatal = true) : base(start, end, "Missing body")
        {
            Fatal = fatal;
        }

        public override bool Fatal { get; }
    }
}