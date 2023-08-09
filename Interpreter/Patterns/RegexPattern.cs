using System.Text.RegularExpressions;
using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Patterns;

internal sealed record RegexPattern : IPatternNode
{
    private readonly string _pattern;

    public RegexPattern(string pattern)
    {
        _pattern = pattern;
    }

    public bool Matches(Value value, Call call)
    {
        if (!String.TryImplicitCast(value, out var @string))
            return false;

        return Regex.Match(@string.Value, _pattern).Success;
    }

    public bool HasAssignment()
    {
        return false;
    }
}