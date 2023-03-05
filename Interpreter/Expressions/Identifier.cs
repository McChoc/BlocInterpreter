using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions;

internal sealed record Identifier : IExpression
{
    internal string Name { get; }

    internal Identifier(string name) => Name = name;

    public IValue Evaluate(Call call) => call.Get(Name);
}