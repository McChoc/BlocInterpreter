using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor
{
    public static partial class Evaluator
    {
        private static IResult EvaluatePrimaries(List<Token> expr, Call call, int precedence)
        {
            IValue value;

            int i = 1;

            switch (expr[0].type)
            {
                case TokenType.Literal:
                    value = (Value)expr[0].value;
                    break;

                case TokenType.Interpolated:
                    {
                        var result = call.Interpolate(expr[0]);

                        if (result is not IValue v)
                            return result;

                        value = v;
                    }
                    break;

                case TokenType.Block:
                    {
                        var result = call.Initialize(expr[0]);

                        if (result is not IValue v)
                            return result;

                        value = v;
                    }
                    break;

                case TokenType.Parentheses:
                    {
                        List<Token> tokens = (List<Token>)expr[0].value;

                        var result = Evaluate(tokens, call);

                        if (result is not IValue v)
                            return result;

                        value = v;
                    }
                    break;

                case TokenType.Identifier:
                    string identifier = expr[0].Text;

                    switch (identifier)
                    {
                        case "global": value = call.Global; break;

                        case "this": value = call.This; break;

                        case "recall": value = call.Recall; break;

                        case "params": value = call.Params; break;

                        default:
                            if (!call.TryGet(identifier, out Pointer ptr))
                                return new Throw($"Variable '{identifier}' was not defined in scope");

                            value = ptr;
                            break;
                    }
                    break;

                default: throw new SyntaxError("Unexpected symbol");
            }

            for (; i < expr.Count; i++)
            {
                if (expr[i] is { type : TokenType.Operator, Text : "." })
                {
                    if (expr.Count <= i)
                        throw new SyntaxError("Missing identifier");

                    Token identifier = expr[i + 1];

                    if (identifier.type != TokenType.Identifier)
                        throw new SyntaxError("Unexpected symbol");

                    if (value is Pointer ptr)
                    {
                        ptr.Accessors.Add(identifier.Text);
                    }
                    else if (value.Value() is Struct obj)
                    {
                        IResult result = obj.Get(new String(identifier.Text), call.Engine);

                        if (result is not IValue v)
                            return result;

                        value = v;
                    }
                    else
                    {
                        throw new SyntaxError("It was not a Struct");
                    }

                    i++;
                }
                else if (expr[i].type == TokenType.Brackets)
                {
                    IResult result = Evaluate((List<Token>)expr[i].value, call);

                    if (result is not IValue v)
                        return result;

                    Value accessor = v.Value();

                    if (value is Pointer ptr && (accessor is Number || accessor is String))
                    {
                        if (accessor is Number num)
                            ptr.Accessors.Add(num.ToInt());
                        else if (accessor is String str)
                            ptr.Accessors.Add(str.Value);

                        value = ptr;
                    }
                    else
                    {
                        switch (value.Value())
                        {
                            case String str:
                                result = str.Get(accessor, call.Engine);
                                break;
                            case Array arr:
                                result = arr.Get(accessor, call.Engine);
                                break;
                            case Struct obj:
                                result = obj.Get(accessor, call.Engine);
                                break;
                            default:
                                return new Throw("It was not a string, an array or a struct");
                        }

                        if (result is not IValue vv)
                            return result;

                        value = vv;
                    }
                }
                else if (expr[i].type == TokenType.Parentheses)
                {
                    if (value.Value().Implicit(out Function func))
                    {
                        var tokens = (List<Token>)expr[i].value;

                        var parameters = new List<Value>();

                        if (tokens.Count > 0)
                        {
                            foreach (var expression in tokens.Split(Token.Comma))
                            {
                                var result = Evaluate(expression, call);

                                if (result is not IValue v)
                                    return result;

                                parameters.Add(v.Value());
                            }
                        }

                        Array array = parameters.Count == 1 && parameters[0] is Array arr ? arr : new Array(parameters.ToList());

                        {
                            var result = func.Call(array, call.Engine);

                            if (result is not IValue v)
                                return result;

                            value = v;
                        }
                    }
                    else
                    {
                        throw new SyntaxError("It was not a function");
                    }
                }
                else
                {
                    throw new SyntaxError("Unexpected symbol");
                }
            }

            return (IResult)value;
        }
    }
}
