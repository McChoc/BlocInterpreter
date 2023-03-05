using Bloc.Values;

namespace Bloc.Variables;

internal sealed class StructVariable : Variable
{
    private readonly Struct _parent;

    public string Name { get; }

    internal StructVariable(string name, Value value, Struct parent)
        : base(true, value)
    {
        _parent = parent;
        Name = name;
    }

    public override void Delete()
    {
        _parent.Values.Remove(Name);

        base.Delete();
    }
}