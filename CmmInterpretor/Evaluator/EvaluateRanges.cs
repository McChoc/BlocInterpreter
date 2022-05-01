using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class Evaluator
    {
        private static IResult EvaluateRanges(List<Token> expr, Call call, int precedence)
        {
            var expressions = expr.Split(new Token(TokenType.Operator, ".."));

            if (expressions.Count == 0)
                throw new SyntaxError("Missing expression");

            if (expressions.Count == 1)
                return Evaluate(expressions[0], call, precedence - 1);

            if (expressions.Count == 2)
            {
                int? start = null, end = null;

                if (expressions[0].Count > 0)
                {
                    var result = Evaluate(expressions[0], call, precedence - 1);

                    if (result is IValue value)
                    {
                        if (value.Implicit(out Number number))
                            start = number.ToInt();
                        else
                            return new Throw("Should be a number");
                    }
                    else
                    {
                        return result;
                    }
                }

                if (expressions[1].Count > 0)
                {
                    var result = Evaluate(expressions[1], call, precedence - 1);

                    if (result is IValue value)
                    {
                        if (value.Implicit(out Number number))
                            end = number.ToInt();
                        else
                            return new Throw("Should be a number");
                    }
                    else
                    {
                        return result;
                    }
                }

                return new Range(start, end);
            }

            if (expressions.Count == 3)
            {
                int? start = null, end = null;
                int step = 1;

                if (expressions[0].Count > 0)
                {
                    var result = Evaluate(expressions[0], call, precedence - 1);

                    if (result is IValue value)
                    {
                        if (value.Implicit(out Number number))
                            start = number.ToInt();
                        else
                            return new Throw("Should be a number");
                    }
                    else
                    {
                        return result;
                    }
                }

                if (expressions[1].Count > 0)
                {
                    var result = Evaluate(expressions[1], call, precedence - 1);

                    if (result is IValue value)
                    {
                        if (value.Implicit(out Number number))
                            end = number.ToInt();
                        else
                            return new Throw("Should be a number");
                    }
                    else
                    {
                        return result;
                    }
                }

                if (expressions[2].Count > 0)
                {
                    var result = Evaluate(expressions[2], call, precedence - 1);

                    if (result is IValue value)
                    {
                        if (value.Implicit(out Number number))
                            step = number.ToInt();
                        else
                            return new Throw("Should be a number");
                    }
                    else
                    {
                        return result;
                    }
                }

                return new Range(start, end, step);
            }

            throw new SyntaxError("Unexpected symbol '..'");
        }
    }
}
