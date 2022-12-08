namespace Bloc.Statements
{
    internal sealed class NewVarStatement : DeclarationStatement
    {
        protected override bool Mask => true;
        protected override bool Mutable => true;
    }
}