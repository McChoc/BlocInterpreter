﻿using System.Collections.Generic;
using System.Globalization;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Number : Value, INumeric
{
    public double Value { get; }

    public Number(double value = 0)
    {
        Value = value;
    }

    public override ValueType GetType() => ValueType.Number;

    public double GetDouble() => Value;

    public int GetInt()
    {
        if (double.IsNaN(Value))
            return 0;

        if (Value > int.MaxValue)
            return int.MaxValue;

        if (Value < int.MinValue)
            return int.MinValue;

        return (int)Value;
    }

    public override string ToString()
    {
        if (double.IsNaN(Value))
            return "nan";

        if (double.IsPositiveInfinity(Value))
            return "infinity";

        if (double.IsNegativeInfinity(Value))
            return "-infinity";

        return Value.ToString(CultureInfo.InvariantCulture);
    }

    internal static Number ImplicitCast(IValue value)
    {
        try
        {
            return Construct(new() { value.Value });
        }
        catch
        {
            throw new Throw($"Cannot implicitly convert '{value.Value.GetTypeName()}' to 'number'");
        }
    }

    internal static bool TryImplicitCast(IValue value, out Number number)
    {
        try
        {
            number = Construct(new() { value.Value });
            return true;
        }
        catch
        {
            number = null!;
            return false;
        }
    }

    internal static Number Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),
            [Number @number] => number,
            [String @string] => new(Parse(@string.Value)),
            [INumeric numeric] => new(numeric.GetDouble()),
            [_] => throw new Throw($"'number' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'number' does not have a constructor that takes {values.Count} arguments")
        };

        static double Parse(string text)
        {
            text = text.Trim();

            if (text == "nan")
                return double.NaN;

            if (text == "infinity")
                return double.PositiveInfinity;

            if (text == "-infinity")
                return double.NegativeInfinity;

            int @base = 10;

            if (text.StartsWith("0b", true, CultureInfo.InvariantCulture))
            {
                @base = 2;
                text = text[2..];
            }
            else if (text.StartsWith("0o", true, CultureInfo.InvariantCulture))
            {
                @base = 8;
                text = text[2..];
            }
            else if (text.StartsWith("0x", true, CultureInfo.InvariantCulture))
            {
                @base = 16;
                text = text[2..];
            }

            if (text.Length < 1 || text[^1] == '_')
                throw new Throw("Input string was not in a correct format");

            text = text.Replace("_", "");

            try
            {
                return @base == 10
                    ? double.Parse(text, CultureInfo.InvariantCulture)
                    : System.Convert.ToInt32(text, @base);
            }
            catch
            {
                throw new Throw("Input string was not in a correct format");
            }
        }
    }
}