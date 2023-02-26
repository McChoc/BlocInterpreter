namespace Bloc.Memory;

public sealed class Scope : VariableCollection
{
    private readonly Call _call;

    public Scope(Call call)
    {
        _call = call;
        _call.Scopes.AddLast(this);
    }

    public override void Dispose()
    {
        base.Dispose();
        _call.Scopes.Remove(this);
    }
}