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
        private static IResult EvaluateAssignments(List<Token> expr, Call call, int precedence)
        {
            for (int i = 0; i < expr.Count; i++)
            {
                if (expr[i] is { type: TokenType.Operator, value: "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "**=" or "//=" or "%%=" or "&&=" or "||=" or "^^=" or "&=" or "|=" or "^=" or "<<=" or ">>=" })
                {
                    string op = expr[i].Text;

                    if (i == 0)
                        throw new SyntaxError("Missing the left part of assignment");

                    if (i > expr.Count - 1)
                        throw new SyntaxError("Missing the right part of assignment");

                    var resultA = Evaluate(expr.GetRange(..i), call, precedence - 1);

                    if (resultA is not IValue a)
                        return resultA;

                    if (a is not Variable var)
                        throw new SyntaxError("You cannot assign a value to a literal");

                    if (op == "=")
                    {
                        var resultB = EvaluateAssignments(expr.GetRange((i + 1)..), call, precedence);

                        if (resultB is not IValue b)
                            return resultB;

                        b = b.Copy();
                        b.Assign();
                        var.Value.Destroy();
                        return var.Value = b.Value;
                    }
                    else if (op == "&&=")
                    {
                        if (!a.Implicit(out Bool boolA))
                            return new Throw("Cannot implicitly convert to bool");

                        if (!boolA.Value)
                            return a.Value;

                        var resultB = EvaluateAssignments(expr.GetRange((i + 1)..), call, precedence);

                        if (resultB is not IValue b)
                            return resultB;

                        if (!b.Implicit(out Bool _))
                            return new Throw("Cannot implicitly convert to bool");

                        b = b.Copy();
                        b.Assign();
                        var.Value.Destroy();
                        return var.Value = b.Value;
                    }
                    else if (op == "||=")
                    {
                        if (!a.Implicit(out Bool boolA))
                            return new Throw("Cannot implicitly convert to bool");

                        if (boolA.Value)
                            return a.Value;

                        var resultB = EvaluateAssignments(expr.GetRange((i + 1)..), call, precedence);

                        if (resultB is not IValue b)
                            return resultB;

                        if (!b.Implicit(out Bool _))
                            return new Throw("Cannot implicitly convert to bool");

                        b = b.Copy();
                        b.Assign();
                        var.Value.Destroy();
                        return var.Value = b.Value;
                    }
                    else if (op == "^^=")
                    {
                        if (!a.Implicit(out Bool boolA))
                            return new Throw("Cannot implicitly convert to bool");

                        var resultB = EvaluateAssignments(expr.GetRange((i + 1)..), call, precedence);

                        if (resultB is not IValue b)
                            return resultB;

                        if (!b.Implicit(out Bool boolB))
                            return new Throw("Cannot implicitly convert to bool");

                        var.Value.Destroy();
                        return var.Value = new Bool(boolA.Value != boolB.Value);
                    }
                    else
                    {
                        var resultB = EvaluateAssignments(expr.GetRange((i + 1)..), call, precedence);

                        if (resultB is not IValue b)
                            return resultB;

                        var result = expr[i].value switch
                        {
                            "+=" => Operator.Add(a, b),
                            "-=" => Operator.Substract(a, b),
                            "*=" => Operator.Multiply(a, b),
                            "/=" => Operator.Divide(a, b),
                            "%=" => Operator.Remainder(a, b),
                            "**=" => Operator.Power(a, b),
                            "//=" => Operator.Root(a, b),
                            "%%=" => Operator.Logarithm(a, b),
                            "&=" => Operator.And(a, b),
                            "|=" => Operator.Or(a, b),
                            "^=" => Operator.Xor(a, b),
                            "<<=" => Operator.Left(a, b),
                            ">>=" => Operator.Right(a, b),
                            _ => throw new System.Exception(),
                        };

                        if (result is not IValue value)
                            return result;

                        value.Assign();
                        var.Value.Destroy();
                        return var.Value = value.Value;
                    }
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
