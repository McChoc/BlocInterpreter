using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class Evaluator
    {
        private static IResult EvaluateUnaries(List<Token> expr, Call call, int precedence)
        {
            if (expr.Count == 0)
                throw new SyntaxError("Missing value");

            if (expr[0].type is TokenType.Operator or TokenType.Keyword)
            {
                var result = EvaluateUnaries(expr.GetRange(1..), call, precedence);

                if (result is not IValue value)
                    return result;

                string op = expr[0].Text;

                if (op == "+")
                    return Operator.Plus(value);
                
                if (op == "-")
                    return Operator.Minus(value);
                
                if (op == "~")
                    return Operator.Reverse(value);

                if (op == "!")
                    return Operator.Not(value);

                if (op == "++")
                {
                    if (result is not Variable var)
                        throw new SyntaxError("The right part of an increment must be a variable");

                    if (!value.Implicit(out Number num))
                        return new Throw("Cannot convert to number");

                    return var.Value = new Number(num.Value + 1);
                }

                if (op == "--")
                {
                    if (result is not Variable var)
                        throw new SyntaxError("The right part of a decrement must be a variable");

                    if (!value.Implicit(out Number num))
                        return new Throw("Cannot convert to number");

                    return var.Value = new Number(num.Value - 1);
                }

                if (op == "~~")
                {
                    if (result is not Variable var)
                        throw new SyntaxError("The right part of an inversion must be a variable");

                    if (value.Implicit(out Number num))
                        return var.Value = new Number(~num.ToInt());

                    //TODO support inversing types

                    return new Throw("Cannot convert to number");
                }

                if (op == "!!")
                {
                    if (result is not Variable var)
                        throw new SyntaxError("The right part of a negation must be a variable");

                    if (!value.Implicit(out Bool b))
                        return new Throw("Cannot convert to bool");

                    return var.Value = new Bool(!b.Value);
                }

                if (op == "len")
                {
                    if (value.Implicit(out Array arr))
                        return new Number(arr.Values.Count);

                    if (value.Implicit(out String str))
                        return new Number(str.Value.Length);

                    throw new SyntaxError("The expression must be a String or an Array");
                }

                if (op == "chr")
                {
                    if (!value.Implicit(out Number num))
                        return new Throw("Cannot convert to number");

                    return new String(((char)num.ToInt()).ToString());
                }

                if (op == "ord")
                {
                    if (!value.Implicit(out String str))
                        return new Throw("Cannot convert to string");

                    if (str.Value.Length != 1)
                        return new Throw("the string must contain only one character");

                    return new Number(str.Value[0]);
                }

                if (op == "val")
                {
                    if (value.Implicit(out Reference r))
                        return (IResult)r.Variable ?? new Throw ("Invalid reference.");

                    return (IResult)value;
                }

                if (op == "ref")
                {
                    if (result is not Variable var)
                        throw new SyntaxError("The right part of a reference must be a variable");

                    var reference = new Reference(var);
                    var.References.Add(reference);
                    return reference;
                }

                if (op == "new")
                {
                    value = value.Copy();
                    value.Assign();

                    var variable = new HeapVariable(value.Value);
                    var reference = new Reference(variable);

                    variable.References.Add(reference);

                    return reference;
                }

                if (op == "nameof")
                {
                    if (EvaluateUnaries(expr.GetRange(1..), call, precedence) is not StackVariable var)
                        throw new SyntaxError("The expression must be a variable stored on the stack");

                    return new String(var.Name);
                }

                if (op == "typeof")
                {
                    return new TypeCollection(value.Type);
                }
            }

            if (expr[^1].type is TokenType.Operator or TokenType.Keyword)
            {
                var result = EvaluateUnaries(expr.GetRange(..^1), call, precedence);

                if (result is not IValue value)
                    return result;

                string op = expr[^1].Text;

                if (op == "++")
                {
                    if (result is not Variable var)
                        throw new SyntaxError("The left part of an increment must be a variable");

                    if (!value.Implicit(out Number num))
                        return new Throw("Cannot convert to number");

                    var.Value = new Number(num.Value + 1);

                    return num;
                }

                if (op == "--")
                {
                    if (result is not Variable var)
                        throw new SyntaxError("The left part of an increment must be a variable");

                    if (!value.Implicit(out Number num))
                        return new Throw("Cannot convert to number");

                    var.Value = new Number(num.Value - 1);

                    return num;
                }

                if (op == "~~")
                {
                    if (result is not Variable var)
                        throw new SyntaxError("The right part of an inversion must be a variable");

                    if (value.Implicit(out Number num))
                    {
                        var.Value = new Number(~num.ToInt());
                        return num;
                    }

                    //TODO support inversing types

                    return new Throw("Cannot convert to number");
                }

                if (op == "!!")
                {
                    if (result is not Variable var)
                        throw new SyntaxError("The right part of a negation must be a variable");

                    if (!value.Implicit(out Bool b))
                        return new Throw("Cannot convert to bool");

                    var.Value = new Bool(!b.Value);
                    return b;
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
