namespace Bloc.Statements
{
    internal sealed class ConstStatement : DeclarationStatement
    {
        protected override bool Mask => false;
        protected override bool Mutable => false;
    }
}