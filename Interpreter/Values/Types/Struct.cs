using System.Collections.Generic;
using System.Linq;
using Bloc.Patterns;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Comparers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Variables;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Struct : Value, IPattern
{
    [DoNotCompare]
    private bool _assigned = false;

    [CompareUsing<ValueEqualityComparer>]
    internal Dictionary<string, IValue> Values { get; private set; }

    public Struct() : this(new()) { }

    public Struct(Dictionary<string, Value> values)
    {
        Values = values.ToDictionary(x => x.Key, x => (IValue)x.Value);
    }

    public override ValueType GetType() => ValueType.Struct;

    public override string ToString()
    {
        return Values.Count > 0
            ? "{" + string.Join(", ", Values.OrderBy(x => x.Key).Select(x => $"{x.Key}: {x.Value.Value}")) + "}"
            : "{&}";
    }

    public IPatternNode GetRoot()
    {
        var patterns = new Dictionary<string, (IPatternNode, bool)>();

        foreach (var (name, member) in Values)
        {
            if (member.Value is not IPattern pattern)
                throw new Throw($"The members of a struct pattern must be patterns");

            patterns.Add(name, (pattern.GetRoot(), false));
        }

        return new StructPattern(patterns, null, false);
    }

    public override void Destroy()
    {
        foreach (var variable in Values.Values.Cast<StructVariable>())
            variable.Delete(true);
    }

    public override Value Copy(bool assign)
    {
        var @struct = new Struct
        {
            _assigned = assign
        };

        @struct.Values = assign
            ? Values.ToDictionary(x => x.Key, x => (IValue)new StructVariable(x.Key, x.Value.Value.Copy(assign), @struct))
            : Values.ToDictionary(x => x.Key, x => (IValue)x.Value.Value.Copy(assign));

        return @struct;
    }

    public override Value GetOrCopy(bool assign)
    {
        var @struct = _assigned
            ? new Struct()
            : this;

        @struct._assigned = assign;
        @struct.Values = assign
            ? Values.ToDictionary(x => x.Key, x => (IValue)new StructVariable(x.Key, x.Value.Value.GetOrCopy(assign), @struct))
            : Values.ToDictionary(x => x.Key, x => (IValue)x.Value.Value.GetOrCopy(assign));

        return @struct;
    }

    public IValue GetMember(string key)
    {
        if (!Values.ContainsKey(key))
            throw new Throw($"'{key}' was not defined inside this struct");

        return _assigned
            ? new VariablePointer((StructVariable)Values[key])
            : Values[key];
    }

    internal static Struct Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),
            [Struct @struct] => @struct,
            [_] => throw new Throw($"'struct' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'struct' does not have a constructor that takes {values.Count} arguments"),
        };
    }
}