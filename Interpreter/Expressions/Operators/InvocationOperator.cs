using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;
using Array = Bloc.Values.Array;

namespace Bloc.Expressions.Operators;

internal sealed class InvocationOperator : IExpression
{
    private readonly IExpression _expression;
    private readonly List<Argument> _arguments;

    internal InvocationOperator(IExpression expression, List<Argument> arguemnts)
    {
        _expression = expression;
        _arguments = arguemnts;
    }

    public IValue Evaluate(Call call)
    {
        var value = _expression.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

        if (value is not IInvokable invokable)
            throw new Throw("The '()' operator can only be applied to a func or a type");

        var args = new List<Value>();
        var kwargs = new Dictionary<string, Value>();

        foreach (var argument in _arguments)
        {
            var val = argument.Expression.Evaluate(call).Value.GetOrCopy();

            switch (argument.Type)
            {
                case ArgumentType.Positional:
                    args.Add(val);
                    break;

                case ArgumentType.Named:
                    if (kwargs.ContainsKey(argument.Name!))
                        throw new Throw($"Parameter named '{argument.Name!}' cannot be specified multiple times");

                    kwargs.Add(argument.Name!, val);
                    break;

                case ArgumentType.UnpackedArray:
                    if (val is not Array array)
                        throw new Throw("Only an array can be unpacked using the array unpack syntax");

                    args.AddRange(array.Values.Select(x => x.Value));
                    break;

                case ArgumentType.UnpackedStruct:
                    if (val is not Struct @struct)
                        throw new Throw("Only a struct can be unpacked using the struct unpack syntax");

                    foreach (var (key, variable) in @struct.Values)
                    {
                        if (kwargs.ContainsKey(key))
                            throw new Throw($"Parameter named '{key}' cannot be specified multiple times");

                        kwargs.Add(key, variable.Value);
                    }
                    break;
            }
        }

        return invokable.Invoke(args, kwargs, call);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_expression, _arguments.Count);
    }

    public override bool Equals(object other)
    {
        return other is InvocationOperator invocation &&
            _expression.Equals(invocation._expression) &&
            _arguments.SequenceEqual(invocation._arguments);
    }

    internal enum ArgumentType
    {
        Positional,
        Named,
        UnpackedArray,
        UnpackedStruct
    }

    internal record Argument
    {
        internal string? Name { get; }
        internal ArgumentType Type { get; }
        internal IExpression Expression { get; }

        internal Argument(string? name, ArgumentType type, IExpression expression)
        {
            Name = name;
            Type = type;
            Expression = expression;
        }
    }
}