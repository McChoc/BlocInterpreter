namespace Bloc.Exceptions
{
    public sealed class MissingSemicolonError : SyntaxError
    {
        public MissingSemicolonError(int start, int end) : base(start, end, "Missing semicolon") { }
    }
}