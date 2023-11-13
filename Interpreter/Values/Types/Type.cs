using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Constants;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Type : Value, IPattern, IInvokable
{
    public ValueType Value { get; }

    public Type(ValueType type)
    {
        Value = type;
    }

    public IPatternNode GetRoot() => new TypePattern(Value);
    public override ValueType GetType() => ValueType.Type;

    public override string ToString()
    {
        return Value switch
        {
            ValueType.Void => Keyword.VOID_T,
            ValueType.Null => Keyword.NULL_T,
            ValueType.Bool => Keyword.BOOL,
            ValueType.Number => Keyword.NUMBER,
            ValueType.Range => Keyword.RANGE,
            ValueType.String => Keyword.STRING,
            ValueType.Array => Keyword.ARRAY,
            ValueType.Struct => Keyword.STRUCT,
            ValueType.Tuple => Keyword.TUPLE,
            ValueType.Func => Keyword.FUNC,
            ValueType.Task => Keyword.TASK,
            ValueType.Iter => Keyword.ITER,
            ValueType.Reference => Keyword.REFERENCE,
            ValueType.Extern => Keyword.EXTERN,
            ValueType.Type => Keyword.TYPE,
            ValueType.Pattern => Keyword.PATTERN,
            _ => throw new System.Exception(),
        };
    }

    public Value Invoke(List<Value?> args, Dictionary<string, Value> kwargs, Call call)
    {
        var values = args
            .OfType<Value>()
            .ToList();

        if (args.Count != values.Count)
            throw new Throw("Constructors do not support default parameters");

        if (kwargs.Count != 0)
            throw new Throw("Constructors do not support named parameters");

        return Value switch
        {
            ValueType.Void => Void.Construct(values),
            ValueType.Null => Null.Construct(values),
            ValueType.Bool => Bool.Construct(values),
            ValueType.Number => Number.Construct(values ),
            ValueType.Range => Range.Construct(values),
            ValueType.String => String.Construct(values),
            ValueType.Array => Array.Construct(values),
            ValueType.Struct => Struct.Construct(values),
            ValueType.Tuple => Tuple.Construct(values),
            ValueType.Func => Func.Construct(values),
            ValueType.Task => Task.Construct(values, call),
            ValueType.Iter => Iter.Construct(values, call),
            ValueType.Reference => Reference.Construct(values),
            ValueType.Extern => Extern.Construct(values),
            ValueType.Type => Construct(values),
            ValueType.Pattern => Pattern.Construct(values),
            _ => throw new System.Exception()
        };
    }

    internal static Type Construct(List<Value> values)
    {
        return values switch
        {
            [Type type] => type,
            [_] => throw new Throw($"'type' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'type' does not have a constructor that takes {values.Count} arguments")
        };
    }
}