using System.Collections.Generic;
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

    public Value Invoke(List<Value> args, Dictionary<string, Value> kwargs, Call call)
    {
        if (kwargs.Count != 0)
            throw new Throw("Constructors do not support named parameters");

        return Value switch
        {
            ValueType.Void => Void.Construct(args),
            ValueType.Null => Null.Construct(args),
            ValueType.Bool => Bool.Construct(args),
            ValueType.Number => Number.Construct(args),
            ValueType.Range => Range.Construct(args),
            ValueType.String => String.Construct(args),
            ValueType.Array => Array.Construct(args),
            ValueType.Struct => Struct.Construct(args),
            ValueType.Tuple => Tuple.Construct(args),
            ValueType.Func => Func.Construct(args),
            ValueType.Task => Task.Construct(args, call),
            ValueType.Iter => Iter.Construct(args, call),
            ValueType.Reference => Reference.Construct(args, call),
            ValueType.Extern => Extern.Construct(args),
            ValueType.Type => Construct(args),
            ValueType.Pattern => Pattern.Construct(args),
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