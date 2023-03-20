using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions;

internal sealed record IdentifierExpression : IExpression
{
    internal string Name { get; }

    internal IdentifierExpression(string name) => Name = name;

    public IValue Evaluate(Call call) => call.Get(Name);
}