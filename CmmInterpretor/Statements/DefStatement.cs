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
                if (definition[0].type == TokenType.Identifier)
                {
                    string identifier = definition[0].Text;
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

                    value.Assign();

                    if (!call.TryAdd(new StackVariable(value, identifier, call.Scopes[^1])))
                        return new Throw($"Variable '{identifier}' was already defined in scope");
                }
                else if (definition[0].type == TokenType.Parentheses)
                {
                    var expressions = ((List<Token>)definition[0].value).Split(Token.Comma);

                    if (expressions.Any(e => e.Count != 1))
                        throw new SyntaxError("Unexpected symbol");

                    if (expressions.Any(e => e[0].type != TokenType.Identifier))
                        throw new SyntaxError("The left part of an assignement must be a variable");

                    var identifiers = expressions.Select(e => e[0].Text).ToList();
                    var values = Enumerable.Repeat<Value>(Null.Value, identifiers.Count).ToList();

                    if (definition.Count > 1)
                    {
                        if (definition[1] is not { type: TokenType.Operator, value: "=" })
                            throw new SyntaxError("Unexpected symbol");

                        var result = Evaluator.Evaluate(definition.GetRange(2..), call);

                        if (result is not IValue value)
                            return result;

                        if (value is Tuple tuple)
                        {
                            if (identifiers.Count != tuple.Values.Count)
                                throw new SyntaxError("Miss match number of elements in tuples.");

                            tuple = (Tuple)tuple.Copy();

                            for (int i = 0; i < identifiers.Count; i++)
                                values[i] = tuple.Values[i].Value;
                        }
                        else
                        {
                            for (int i = 0; i < identifiers.Count; i++)
                                values[i] = value.Copy();
                        }
                    }

                    foreach (var (identifier, value) in identifiers.Zip(values, (i, v) => (i, v)))
                    {
                        value.Assign();

                        if (!call.TryAdd(new StackVariable(value, identifier, call.Scopes[^1])))
                            return new Throw($"Variable '{identifier}' was already defined in scope");
                    }
                }
                else
                {
                    throw new SyntaxError("The left part of an assignement must be a variable");
                }
            }

            return Void.Value;
        }

        //public struct Decorator
        //{
        //    public string identifier;
        //    public List<List<Token>> arguments;
        //}
    }
}
