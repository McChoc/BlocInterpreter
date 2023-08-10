using System.Collections.Generic;
using Bloc.Memory;

namespace Bloc.Commands.Arguments;

internal sealed record LiteralArgument : IArgument
{
    private readonly string _text;

    internal LiteralArgument(string text)
    {
        _text = text;
    }

    public IEnumerable<string> GetArguments(Call _)
    {
        yield return _text;
    }
}