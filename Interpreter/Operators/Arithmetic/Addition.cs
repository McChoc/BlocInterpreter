using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators
{
    internal class Addition : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Addition(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(left, right, Operation, call);
        }

        internal static Value Operation(Value left, Value right)
        {
            if (left.Is(out Number? leftNumber) && right.Is(out Number? rightNumber))
                return new Number(leftNumber!.Value + rightNumber!.Value);

            if (left.Is(out Struct? leftStruct) && right.Is(out Struct? rightStruct))
            {
                var dict = new Dictionary<string, IVariable>();

                foreach (var (key, value) in ((Struct)leftStruct!.Copy()).Values)
                    dict[key] = value;

                foreach (var (key, value) in ((Struct)rightStruct!.Copy()).Values)
                    dict[key] = value;

                return new Struct(dict);
            }

            if (left.Is(out Array? leftArray) && right.Is(out Array? rightArray))
            {
                var list = new List<IVariable>(leftArray!.Values.Count + rightArray!.Values.Count);
                list.AddRange(((Array)leftArray.Copy()).Values);
                list.AddRange(((Array)rightArray.Copy()).Values);
                return new Array(list);
            }

#pragma warning disable IDE0028

            if (left.Is(out Array? array))
            {
                var list = new List<IVariable>(array!.Values.Count + 1);
                list.AddRange(((Array)array.Copy()).Values);
                list.Add(right.Copy());
                return new Array(list);
            }

            if (right.Is(out array))
            {
                var list = new List<IVariable>(array!.Values.Count + 1);
                list.Add(left.Copy());
                list.AddRange(((Array)array.Copy()).Values);
                return new Array(list);
            }

#pragma warning restore IDE0028

            if (left.Is(out String? leftString) && right.Is(out String? rightString))
                return new String(leftString!.Value + rightString!.Value);

            throw new Throw($"Cannot apply operator '+' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}