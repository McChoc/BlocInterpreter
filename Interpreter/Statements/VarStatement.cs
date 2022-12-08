namespace Bloc.Statements
{
    internal sealed class VarStatement : DeclarationStatement
    {
        protected override bool Mask => false;
        protected override bool Mutable => true;
    }
}