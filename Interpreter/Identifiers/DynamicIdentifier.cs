using System.Text.RegularExpressions;
using Bloc.Core;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;
using Bloc.Variables;

namespace Bloc.Identifiers;

internal sealed record DynamicIdentifier : INamedIdentifier
{
    private const string RESERVED_CHARACTERS = "!\"#$%&'()*+,\\-.\\/:;<=>?@\\[\\\\\\]^`{|}~";
    private const string IDENTIFIER_REGEX = $"^[^0-9{RESERVED_CHARACTERS}][^{RESERVED_CHARACTERS}]*$";

    private readonly IExpression _exepression;

    public DynamicIdentifier(IExpression exepression)
    {
        _exepression = exepression;
    }

    public string GetName(Call call)
    {
        var value = _exepression.Evaluate(call);

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit);

        var @string = String.ImplicitCast(value);

        if (!Regex.Match(@string.Value, IDENTIFIER_REGEX).Success)
            throw new Throw("Invalid Identifier name");

        return @string.Value;
    }

    public Value From(Module module, Call call)
    {
        var name = GetName(call);

        if (!module.Exports.TryGetValue(name, out var export))
            throw new Throw($"Module '{module.Path}' does not export {name}");

        return export.Copy();
    }

    public IValue Define(Value value, Call call, bool mask, bool mutable, VariableScope scope)
    {
        var name = GetName(call);

        return call.Set(name, value.GetOrCopy(true), mutable, mask, scope);
    }
}