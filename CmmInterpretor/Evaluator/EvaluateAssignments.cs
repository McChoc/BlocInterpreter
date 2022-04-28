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
                if (expr[i].type == TokenType.Operator)
                {
                    string op = expr[i].Text;

                    if (op is "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "**=" or "//=" or "%%=" or "&&=" or "||=" or "^^=" or "&=" or "|=" or "^=" or "<<=" or ">>=")
                    {
                        if (i == 0)
                            throw new SyntaxError("Missing the left part of assignment");

                        if (i > expr.Count - 1)
                            throw new SyntaxError("Missing the right part of assignment");

                        if (op == "=")
                        {
                            var a = Evaluate(expr.GetRange(..i), call, precedence - 1);

                            if (a is not IValue)
                                return a;

                            if (a is not Pointer ptr)
                                throw new SyntaxError("You cannot assign a value to a literal");

                            var b = EvaluateAssignments(expr.GetRange((i + 1)..), call, precedence);

                            if (b is not IValue bb)
                                return b;

                            return ptr.Set(bb.Value());
                        }
                        else if (op == "&&=")
                        {
                            var a = Evaluate(expr.GetRange(..i), call, precedence - 1);

                            if (a is not IValue aa)
                                return a;

                            if (a is not Pointer ptr)
                                throw new SyntaxError("You cannot assign a value to a literal");

                            if (!aa.Value().Implicit(out Bool aaa))
                                return new Throw("Cannot implicitly convert to bool");

                            if (!aaa.Value)
                                return aa.Value();

                            var b = EvaluateAssignments(expr.GetRange((i + 1)..), call, precedence);

                            if (b is not IValue bb)
                                return b;

                            return ptr.Set(bb.Value());
                        }
                        else if (op == "||=")
                        {
                            var a = Evaluate(expr.GetRange(..i), call, precedence - 1);

                            if (a is not IValue aa)
                                return a;

                            if (a is not Pointer ptr)
                                throw new SyntaxError("You cannot assign a value to a literal");

                            if (!aa.Value().Implicit(out Bool aaa))
                                return new Throw("Cannot implicitly convert to bool");

                            if (aaa.Value)
                                return aa.Value();

                            var b = EvaluateAssignments(expr.GetRange((i + 1)..), call, precedence);

                            if (b is not IValue bb)
                                return b;

                            return ptr.Set(bb.Value());
                        }
                        else if (op == "^^=")
                        {
                            var a = Evaluate(expr.GetRange(..i), call, precedence - 1);

                            if (a is not IValue aa)
                                return a;

                            if (a is not Pointer ptr)
                                throw new SyntaxError("You cannot assign a value to a literal");

                            if (!aa.Value().Implicit(out Bool aaa))
                                return new Throw("Cannot implicitly convert to bool");

                            var b = EvaluateAssignments(expr.GetRange((i + 1)..), call, precedence);

                            if (b is not IValue bb)
                                return b;

                            if (!bb.Value().Implicit(out Bool bbb))
                                return new Throw("Cannot implicitly convert to bool");

                            return ptr.Set(new Bool(aaa.Value != bbb.Value));
                        }
                        else
                        {
                            var a = Evaluate(expr.GetRange(..i), call, precedence - 1);

                            if (a is not IValue)
                                return a;

                            if (a is not Pointer ptr)
                                throw new SyntaxError("You cannot assign a value to a literal");

                            var b = EvaluateAssignments(expr.GetRange((i + 1)..), call, precedence);

                            if (b is not IValue value)
                                return b;

                            System.Func<Value, Value, IResult> operation = expr[i].value switch
                            {
                                "+=" => Operator.Add,
                                "-=" => Operator.Substract,
                                "*=" => Operator.Multiply,
                                "/=" => Operator.Divide,
                                "%=" => Operator.Remainder,
                                "**=" => Operator.Power,
                                "//=" => Operator.Root,
                                "%%=" => Operator.Logarithm,
                                "&=" => Operator.And,
                                "|=" => Operator.Or,
                                "^=" => Operator.Xor,
                                "<<=" => Operator.Left,
                                ">>=" => Operator.Right,
                                _ => throw new System.Exception(),
                            };

                            var result = operation(ptr.Variable.value, value.Value());

                            if (result is not IValue v)
                                return result;

                            return ptr.Set(v.Value());
                        }
                    }
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
