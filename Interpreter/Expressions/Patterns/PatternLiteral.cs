using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Patterns;

internal sealed record PatternLiteral : IExpression
{
    private readonly IPatternNode _root;

    internal PatternLiteral(IPatternNode root) => _root = root;

    public IValue Evaluate(Call _) => new Pattern(_root);
}