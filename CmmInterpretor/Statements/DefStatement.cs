using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Statements
{
    public class DefStatement : Statement
    {
        public List<List<Token>> definitions;

        //internal List<Decorator> decorators = new List<Decorator>();

        public override IResult Execute(Call call)
        {
            //var computedDecorators = decorators.Select(d => new CmmInterpretor.Decorator() { pointer = call.Find(d.identifier), arguments = d.arguments.Select(a => Evaluator.Evaluate(a, call).Value()).ToList() }).ToList();

            foreach (var definition in definitions)
            {
                Token identifier = definition[0];
                Value value = Null.Value;

                if (definition.Count > 1)
                {
                    if (definition[1] is not { type: TokenType.Operator, value: "=" })
                        throw new SyntaxError("Unexpected symbol");

                    var result = Evaluator.Evaluate(definition.GetRange(2..), call);

                    if (result is not IValue v)
                        return result;

                    value = v.Copy();
                }

                {
                    var result = Define(identifier, value, call);

                    if (result is IValue)
                        return result;
                }
            }

            return Void.Value;
        }

        private IResult Define(Token token, Value value, Call call)
        {
            if (token is { type: TokenType.Identifier, value: string name })
            {
                value.Assign();

                if (!call.TryAdd(new StackVariable(value, name, call.Scopes[^1])))
                    return new Throw($"Variable '{token}' was already defined in scope");

                return Void.Value;
            }
            
            if (token is { type: TokenType.Parentheses, value: List<Token> expression })
            {
                var expressions = expression.Split(Token.Comma);

                if (expressions.Any(e => e.Count != 1))
                    throw new SyntaxError("Unexpected symbol");

                var identifiers = expressions.Select(e => e[0]).ToList();

                if (!value.Implicit(out Tuple tuple))
                {
                    foreach (var identifier in identifiers)
                    {
                        var result = Define(identifier, value, call);

                        if (result is not IValue)
                            return result;
                    }
                }
                else
                {
                    if (identifiers.Count != tuple.Values.Count)
                        throw new SyntaxError("Miss match number of elements in tuples.");

                    foreach (var (identifier, val) in identifiers.Zip(tuple.Values.Select(v => v.Value), (i, v) => (i, v)))
                    {
                        var result = Define(identifier, val, call);

                        if (result is not IValue)
                            return result;
                    }
                }

                return Void.Value;
            }
            
            throw new SyntaxError("The left part of an assignement must be a variable");
        }

        //public struct Decorator
        //{
        //    public string identifier;
        //    public List<List<Token>> arguments;
        //}
    }
}
