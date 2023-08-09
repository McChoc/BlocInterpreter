using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Utils.Helpers;

internal delegate Value UnaryOperation(Value value);

internal delegate Value BinaryOperation(Value left, Value right);

internal static class OperatorHelper
{
    internal static bool IsBinary(List<Token> tokens, int index)
    {
        for (int i = index - 1; i >= 0; i--)
        {
            if (tokens[i] is SymbolToken(Symbol.INCREMENT or Symbol.DECREMENT or Symbol.BIT_INV or Symbol.BOOL_INV))
                continue;

            return tokens[i] is not SymbolToken and not KeywordToken;
        }

        return false;
    }

    internal static Value RecursivelyCall(Value value, UnaryOperation operation, Call call)
    {
        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is Tuple tuple)
            return new Tuple(tuple.Values
                .Select(x => RecursivelyCall(x.Value, operation, call))
                .ToList());

        return operation(value);
    }

    internal static Value RecursivelyCall(Value left, Value right, BinaryOperation operation, Call call)
    {
        left = ReferenceHelper.Resolve(left, call.Engine.Options.HopLimit).Value;
        right = ReferenceHelper.Resolve(right, call.Engine.Options.HopLimit).Value;

        switch (left, right)
        {
            case (Tuple leftTuple, Tuple rightTuple):
                if (leftTuple.Values.Count != rightTuple.Values.Count)
                    throw new Throw("Miss mathch number of elements inside the tuples");

                return new Tuple(leftTuple.Values
                    .Zip(rightTuple.Values, (a, b) => RecursivelyCall(a.Value, b.Value, operation, call))
                    .ToList());

            case (Tuple tuple, _):
                return new Tuple(tuple.Values
                    .Select(x => RecursivelyCall(x.Value, right, operation, call))
                    .ToList());

            case (_, Tuple tuple):
                return new Tuple(tuple.Values
                    .Select(x => RecursivelyCall(left, x.Value, operation, call))
                    .ToList());

            default:
                return operation(left, right);
        }
    }
}