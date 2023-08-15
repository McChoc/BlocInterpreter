﻿using Bloc.Values.Core;
using Bloc.Values.Types;

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

    public override void Delete(bool deleting)
    {
        if (!deleting)
            _parent.Values.Remove(Name);

        base.Delete();
    }
}