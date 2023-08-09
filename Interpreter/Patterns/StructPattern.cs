using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Patterns;

internal sealed class StructPattern : IPatternNode
{
    private readonly bool _hasPack;
    private readonly IPatternNode? _packPattern;
    private readonly Dictionary<string, IPatternNode> _patterns;

    internal StructPattern(Dictionary<string, IPatternNode> patterns, IPatternNode? packPattern, bool hasPack)
    {
        _patterns = patterns;
        _packPattern = packPattern;
        _hasPack = hasPack;
    }

    public bool Matches(Value value, Call call)
    {
        if (value is not Struct @struct)
            return false;

        if (!_hasPack)
        {
            if (@struct.Values.Count != _patterns.Count)
                return false;
        }
        else
        {
            if (@struct.Values.Count - _patterns.Count < 0)
                return false;
        }

        var unmatchedMembers = @struct.Values
            .ToDictionary(x => x.Key, x => x.Value.Value);

        foreach (var (key, pattern) in _patterns)
        {
            if (!unmatchedMembers.Remove(key, out var subValue))
                return false;

            if (!pattern.Matches(subValue, call))
                return false;
        }

        if (_packPattern is not null)
        {
            var packedValue = new Struct(unmatchedMembers);

            if (!_packPattern.Matches(packedValue, call))
                return false;
        }

        return true;
    }

    public bool HasAssignment()
    {
        return _patterns.Values.Any(x => x.HasAssignment()) ||
            (_packPattern?.HasAssignment() ?? false);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_packPattern, _patterns.Count);
    }

    public override bool Equals(object other)
    {
        if (other is not StructPattern pattern)
            return false;

        if (_hasPack != pattern._hasPack)
            return false;

        if (!Equals(_packPattern, pattern._packPattern))
            return false;

        if (_patterns.Count != pattern._patterns.Count)
            return false;

        foreach (var key in _patterns.Keys)
        {
            if (!pattern._patterns.TryGetValue(key, out var value))
                return false;

            if (_patterns[key] != value)
                return false;
        }

        return true;
    }
}