using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;

namespace Bloc.Expressions.Operators;

internal sealed record FromOperator : IExpression
{
    private readonly IIdentifier _identifier;
    private readonly IExpression _expression;

    internal FromOperator(IIdentifier identifier, IExpression expression)
    {
        _identifier = identifier;
        _expression = expression;
    }

    public IValue Evaluate(Call call)
    {
        string path = ImportHelper.ResolveModulePath(_expression, call);
        var module = ImportHelper.GetModule(path, call);

        return _identifier.From(module, call);
    }
}