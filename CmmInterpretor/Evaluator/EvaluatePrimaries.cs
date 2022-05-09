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

            switch (expr[0])
            {
                case { type: TokenType.Literal }:
                    value = (Value)expr[0].value;
                    break;

                case { type: TokenType.Interpolated }:
                    {
                        var result = call.Interpolate(expr[0]);

                        if (result is not IValue v)
                            return result;

                        value = v;
                    }
                    break;

                case { type: TokenType.Block }:
                    {
                        var result = call.Initialize(expr[0]);

                        if (result is not IValue v)
                            return result;

                        value = v;
                    }
                    break;

                case { type: TokenType.Parentheses }:
                    {
                        List<Token> tokens = (List<Token>)expr[0].value;

                        var result = Evaluate(tokens, call);

                        if (result is not IValue v)
                            return result;

                        value = v;
                    }
                    break;

                case { type: TokenType.Identifier, value: "global" }:
                    value = call.Global;
                    break;

                case { type: TokenType.Identifier, value: "this" }:
                    value = call.This;
                    break;

                case { type: TokenType.Identifier, value: "recall" }:
                    value = call.Recall;
                    break;

                case { type: TokenType.Identifier, value: "params" }:
                    value = call.Params;
                    break;

                case { type: TokenType.Identifier, value: string identifier }:
                    {
                        if (!call.TryGet(identifier, out Variable var))
                            return new Throw($"Variable '{identifier}' was not defined in scope");

                        value = var;
                    }
                    break;

                default:
                    throw new SyntaxError("Unexpected symbol");
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

                    if (value.Implicit(out Struct obj))
                    {
                        IResult result = obj.Get(identifier.Text);

                        if (result is not IValue v)
                            return result;

                        value = v;
                    }
                    else
                    {
                        throw new SyntaxError("The '.' operator can only be apllied to a struct");
                    }

                    i++;
                }
                else if (expr[i].type == TokenType.Brackets)
                {
                    if (value.Value is not IIndexable indx)
                        return new Throw("You can only index a string, an array or a struct.");

                    IResult result = Evaluate((List<Token>)expr[i].value, call);

                    if (result is not IValue v)
                        return result;

                    Value accessor = v.Value;

                    result = indx.Index(accessor, call.Engine);

                    if (result is not IValue vv)
                        return result;

                    value = vv;
                }
                else if (expr[i].type == TokenType.Parentheses)
                {
                    if (value.Value is not IInvokable invk)
                        return new Throw("You can only invoke a function or a type.");

                    var tokens = (List<Token>)expr[i].value;

                    var parameters = new List<Value>();

                    if (tokens.Count > 0)
                    {
                        foreach (var expression in tokens.Split(Token.Comma))
                        {
                            var result = Evaluate(expression, call);

                            if (result is not IValue v)
                                return result;

                            parameters.Add(v.Value);
                        }
                    }

                    {
                        var values = parameters.Count == 1 && parameters[0] is Array arr ? arr.Values.Cast<Value>().ToList() : parameters;

                        var result = invk.Invoke(values, call.Engine);

                        if (result is not IValue v)
                            return result;

                        value = v;
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
