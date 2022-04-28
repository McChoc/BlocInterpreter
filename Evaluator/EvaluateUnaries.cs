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
                    return Operator.Positive(value.Value());
                
                if (op == "-")
                    return Operator.Negative(value.Value());
                
                if (op == "~")
                    return Operator.Reverse(value.Value());

                if (op == "!")
                    return Operator.Not(value.Value());

                if (op == "++")
                {
                    if (result is not Pointer ptr)
                        throw new SyntaxError("The right part of an increment must be a variable");

                    if (!value.Value().Implicit(out Number num))
                        return new Throw("Cannot convert to number");

                    return ptr.Set(new Number(num.Value + 1));
                }

                if (op == "--")
                {
                    if (result is not Pointer ptr)
                        throw new SyntaxError("The right part of a decrement must be a variable");

                    if (!value.Value().Implicit(out Number num))
                        return new Throw("Cannot convert to number");

                    return ptr.Set(new Number(num.Value - 1));
                }

                if (op == "~~")
                {
                    if (result is not Pointer ptr)
                        throw new SyntaxError("The right part of an inversion must be a variable");

                    if (value.Value().Implicit(out Number num))
                        return ptr.Set(new Number(~num.ToInt()));

                    //TODO support inversing types

                    return new Throw("Cannot convert to number");
                }

                if (op == "!!")
                {
                    if (result is not Pointer ptr)
                        throw new SyntaxError("The right part of a negation must be a variable");

                    if (!value.Value().Implicit(out Bool b))
                        return new Throw("Cannot convert to bool");

                    return ptr.Set(new Bool(!b.Value));
                }

                if (op == "len")
                {
                    if (value.Value().Implicit(out Array arr))
                        return new Number(arr.Variables.Count);

                    if (value.Value().Implicit(out String str))
                        return new Number(str.Value.Length);

                    throw new SyntaxError("The expression must be a String or an Array");
                }

                if (op == "chr")
                {
                    if (!value.Value().Implicit(out Number num))
                        return new Throw("Cannot convert to number");

                    return new String(((char)num.ToInt()).ToString());
                }

                if (op == "ord")
                {
                    if (!value.Value().Implicit(out String str))
                        return new Throw("Cannot convert to string");

                    if (str.Value.Length != 1)
                        return new Throw("the string must contain only one character");

                    return new Number(str.Value[0]);
                }

                if (op == "val")
                {
                    if (value.Value().Implicit(out Reference r))
                        return (IResult)r.Pointer ?? new Throw ("Invalid reference.");

                    return (IResult)value;
                }

                if (op == "ref")
                {
                    if (result is not Pointer ptr)
                        throw new SyntaxError("The right part of a reference must be a variable");

                    return new Reference(ptr);
                }

                if (op == "new")
                {
                    var variable = new Variable(null, value.Value().Copy(), null);
                    var pointer = new Pointer(variable, call.Engine);
                    var reference = new Reference(pointer);

                    variable.References.Add(reference);

                    return reference;
                }

                if (op == "del")
                {
                    if (!value.Value().Implicit(out Reference r))
                        return new Throw("Cannot convert to reference");

                    if (r.Pointer.Variable.name != null)
                        return new Throw("The reference have to point to a variable stored on the heap.");

                    var val = r.Pointer.Get();

                    r.Pointer.Remove();

                    return val;
                }

                if (op == "nameof")
                {
                    if (EvaluateUnaries(expr.GetRange(1..), call, precedence) is not Pointer ptr)
                        throw new SyntaxError("The expression must be a variable");

                    return ptr.NameOf();
                }

                if (op == "typeof")
                {
                    return new Type(value.Value().TypeOf());
                }
            }

            if (expr[^1].type == TokenType.Operator || expr[^1].type == TokenType.Keyword)
            {
                var result = EvaluateUnaries(expr.GetRange(..^1), call, precedence);

                if (result is not IValue value)
                    return result;

                string op = expr[^1].Text;

                if (op == "++")
                {
                    if (result is not Pointer ptr)
                        throw new SyntaxError("The left part of an increment must be a variable");

                    if (!value.Value().Implicit(out Number num))
                        return new Throw("Cannot convert to number");

                    ptr.Set(new Number(num.Value + 1));

                    return num;
                }

                if (op == "--")
                {
                    if (result is not Pointer ptr)
                        throw new SyntaxError("The left part of an increment must be a variable");

                    if (!value.Value().Implicit(out Number num))
                        return new Throw("Cannot convert to number");

                    ptr.Set(new Number(num.Value - 1));

                    return num;
                }

                if (op == "~~")
                {
                    if (result is not Pointer ptr)
                        throw new SyntaxError("The right part of an inversion must be a variable");

                    if (value.Value().Implicit(out Number num))
                    {
                        ptr.Set(new Number(~num.ToInt()));
                        return num;
                    }

                    //TODO support inversing types

                    return new Throw("Cannot convert to number");
                }

                if (op == "!!")
                {
                    if (result is not Pointer ptr)
                        throw new SyntaxError("The right part of a negation must be a variable");

                    if (!value.Value().Implicit(out Bool b))
                        return new Throw("Cannot convert to bool");

                    ptr.Set(new Bool(!b.Value));
                    return b;
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
