using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Variables;

internal sealed class ArrayVariable : Variable
{
    private readonly Array _parent;

    internal ArrayVariable(Value value, Array parent)
        : base(true, value)
    {
        _parent = parent;
    }

    public override void Delete(bool deleting)
    {
        if (!deleting)
            _parent.Values.Remove(this);

        base.Delete();
    }
}