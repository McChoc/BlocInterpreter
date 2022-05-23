namespace CmmInterpretor.Utils.Exceptions
{
    public class MissingSemicolonError : SyntaxError
    {
        public MissingSemicolonError() : base("Missing semicolon") { }
    }
}
