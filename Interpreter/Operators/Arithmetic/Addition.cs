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
            var left = _left.Evaluate(call);
            var right = _right.Evaluate(call);

            return TupleUtil.RecursivelyCall(left, right, Operation);
        }

        internal static IPointer Operation(IPointer left, IPointer right)
        {
            if (left.Value.Is(out Number? leftNumber) && right.Value.Is(out Number? rightNumber))
                return new Number(leftNumber!.Value + rightNumber!.Value);

            if (left.Value.Is(out Struct? leftStruct) && right.Value.Is(out Struct? rightStruct))
            {
                var dict = new Dictionary<string, IVariable>();

                foreach (var (key, value) in ((Struct)leftStruct!.Copy()).Values)
                    dict[key] = value;

                foreach (var (key, value) in ((Struct)rightStruct!.Copy()).Values)
                    dict[key] = value;

                return new Struct(dict);
            }

            if (left.Value.Is(out Array? leftArray) && right.Value.Is(out Array? rightArray))
            {
                var list = new List<IVariable>(leftArray!.Values.Count + rightArray!.Values.Count);
                list.AddRange(((Array)leftArray.Copy()).Values);
                list.AddRange(((Array)rightArray.Copy()).Values);
                return new Array(list);
            }

#pragma warning disable IDE0028

            if (left.Value.Is(out Array? array))
            {
                var list = new List<IVariable>(array!.Values.Count + 1);
                list.AddRange(((Array)array.Copy()).Values);
                list.Add(right.Value.Copy());
                return new Array(list);
            }

            if (right.Value.Is(out array))
            {
                var list = new List<IVariable>(array!.Values.Count + 1);
                list.Add(left.Value.Copy());
                list.AddRange(((Array)array.Copy()).Values);
                return new Array(list);
            }

#pragma warning restore IDE0028

            if (left.Value.Is(out String? leftString) && right.Value.Is(out String? rightString))
                return new String(leftString!.Value + rightString!.Value);

            throw new Throw($"Cannot apply operator '+' on operands of types {left.Value.GetType().ToString().ToLower()} and {right.Value.GetType().ToString().ToLower()}");
        }
    }
}