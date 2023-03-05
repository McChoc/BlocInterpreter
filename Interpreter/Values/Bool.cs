using System;
using System.Collections.Generic;
using Bloc.Interfaces;
using Bloc.Results;

namespace Bloc.Values;

public sealed class Bool : Value, IScalar
{
    public static Bool True { get; } = new(true);
    public static Bool False { get; } = new(false);

    public bool Value { get; }

    internal Bool(bool value) => Value = value;

    public int GetInt() => Value ? 1 : 0;
    public double GetDouble() => Value ? 1 : 0;
    internal override ValueType GetType() => ValueType.Bool;

    internal static Bool Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => False,
            [Bool @bool] => @bool,
            [Number number] => new(number.Value is not (0 or double.NaN)),
            [Void] => throw new Throw($"'bool' does not have a constructor that takes a 'void'"),
            [_] => True,
            [..] => throw new Throw($"'bool' does not have a constructor that takes {values.Count} arguments")
        };
    }

    internal static Bool ImplicitCast(Value value)
    {
        try
        {
            return Construct(new() { value });
        }
        catch
        {
            throw new Throw($"Cannot implicitly convert '{value.GetType().ToString().ToLower()}' to 'bool'");
        }
    }

    internal static bool TryImplicitCast(Value value, out Bool @bool)
    {
        try
        {
            @bool = Construct(new() { value });
            return true;
        }
        catch
        {
            @bool = null!;
            return false;
        }
    }

    public override string ToString()
    {
        return Value ? "true" : "false";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public override bool Equals(object other)
    {
        return other is Bool @bool &&
            Value == @bool.Value;
    }
}