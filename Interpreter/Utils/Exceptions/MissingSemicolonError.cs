namespace Bloc.Utils.Exceptions
{
    public class MissingSemicolonError : SyntaxError
    {
        public MissingSemicolonError(int start, int end) : base(start, end, "Missing semicolon")
        {
        }
    }
}