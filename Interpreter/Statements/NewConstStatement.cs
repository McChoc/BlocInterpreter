namespace Bloc.Statements
{
    internal sealed class NewConstStatement : DeclarationStatement
    {
        protected override bool Mask => true;
        protected override bool Mutable => false;
    }
}