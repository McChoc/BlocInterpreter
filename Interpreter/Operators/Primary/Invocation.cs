using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed class Invocation : IExpression
    {
        private readonly IExpression _expression;
        private readonly List<Argument> _arguments;

        internal Invocation(IExpression expression, List<Argument> arguemnts)
        {
            _expression = expression;
            _arguments = arguemnts;
        }

        public IValue Evaluate(Call call)
        {
            var value = _expression.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is not IInvokable invokable)
                throw new Throw("The '()' operator can only be applied to a func or a type");

            var args = new List<Value>();
            var kwargs = new Dictionary<string, Value>();

            foreach (var argument in _arguments)
            {
                var val = argument.Expression.Evaluate(call).Value.Copy();

                switch (argument.Type)
                {
                    case ArgumentType.Positional:
                        args.Add(val);
                        break;
                    case ArgumentType.Named:
                        if (!kwargs.ContainsKey(argument.Name!))
                            kwargs.Add(argument.Name!, val);
                        else
                            throw new Throw($"Parameter named '{argument.Name!}' cannot be specified multiple times");
                        break;
                    case ArgumentType.UnpackedArray:
                        if (val is Array array)
                            args.AddRange(array.Variables.Select(x => x.Value));
                        else
                            throw new Throw("Only an array can be unpacked using the array unpack syntax");
                        break;
                    case ArgumentType.UnpackedStruct:
                        if (val is Struct @struct)
                            foreach (var (key, variable) in @struct.Variables)
                                if (!kwargs.ContainsKey(key))
                                    kwargs.Add(key, variable.Value);
                                else
                                    throw new Throw($"Parameter named '{key}' cannot be specified multiple times");
                        else
                            throw new Throw("Only a struct can be unpacked using the struct unpack syntax");
                        break;
                }
            }

            return invokable.Invoke(args, kwargs, call);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_expression, _arguments.Count);
        }

        public override bool Equals(object other)
        {
            return other is Invocation invocation &&
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
}