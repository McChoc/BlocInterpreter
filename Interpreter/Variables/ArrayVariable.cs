using Bloc.Values;

namespace Bloc.Variables;

internal sealed class ArrayVariable : Variable
{
    private readonly Array _parent;

    internal ArrayVariable(Value value, Array parent)
        : base(true, value)
    {
        _parent = parent;
    }

    public override void Delete()
    {
        _parent.Values.Remove(this);

        base.Delete();
    }
}