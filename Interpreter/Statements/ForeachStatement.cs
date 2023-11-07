using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Types;

namespace Bloc.Statements;

[Record]
internal sealed partial class ForeachStatement : Statement
{
    private readonly bool _checked;

    internal required IIdentifier Identifier { get; init; }
    internal required IExpression Expression { get; init; }
    internal required Statement Statement { get; init; }

    internal ForeachStatement(bool @checked)
    {
        _checked = @checked;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(Expression, call, out var value, out var exception))
        {
            yield return exception;
            yield break;
        }

        if (!Iter.TryImplicitCast(value, out var iter, call))
        {
            yield return new Throw("Cannot implicitly convert to iter");
            yield break;
        }

        using var enumerator = iter.Iterate().GetEnumerator();

        int loopCount = 0;

        while (true)
        {
            try
            {
                if (!enumerator.MoveNext())
                    break;
            }
            catch (Throw e)
            {
                exception = e;
            }

            if (exception is not null)
            {
                yield return exception;
                yield break;
            }

            if (++loopCount > call.Engine.Options.LoopLimit && _checked)
            {
                yield return new Throw("The loop limit was reached.");
                yield break;
            }

            bool @break = false;

            using (call.MakeScope())
            {
                if (!DefineIdentifier(Identifier, enumerator.Current, call, out exception, mutable: false))
                {
                    yield return exception;
                    yield break;
                }

                foreach (var result in ExecuteStatement(Statement, call))
                {
                    bool @continue = false;

                    switch (result)
                    {
                        case Continue:
                            @continue = true;
                            break;

                        case Break:
                            @break = true;
                            break;

                        case Yield:
                            yield return result;
                            break;

                        default:
                            yield return result;
                            yield break;
                    }

                    if (@continue || @break)
                        break;
                }
            }

            if (@break)
                break;
        }
    }
}