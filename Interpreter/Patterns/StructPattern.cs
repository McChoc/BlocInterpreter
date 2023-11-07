using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Patterns;

[Record]
internal sealed partial class StructPattern : IPatternNode
{
    private readonly bool _hasPack;
    private readonly IPatternNode? _packPattern;
    private readonly Dictionary<string, (IPatternNode Pattern, bool Optional)> _members;

    internal StructPattern(Dictionary<string, (IPatternNode, bool)> members, IPatternNode? packPattern, bool hasPack)
    {
        _members = members;
        _packPattern = packPattern;
        _hasPack = hasPack;
    }

    public bool Matches(Value value, Call call)
    {
        if (value is not Struct @struct)
            return false;

        var unmatchedMembers = @struct.Values
            .ToDictionary(x => x.Key, x => x.Value.Value);

        foreach (var (key, (pattern, optional)) in _members)
        {
            if (unmatchedMembers.Remove(key, out var subValue))
            {
                if (!pattern.Matches(subValue, call))
                    return false;
            }
            else if (!optional)
            {
                return false;
            }
        }

        if (!_hasPack && unmatchedMembers.Count > 0)
            return false;

        if (_packPattern is null)
            return true;

        var packedValue = new Struct(unmatchedMembers);

        return _packPattern.Matches(packedValue, call);
    }

    public bool HasAssignment()
    {
        return _members.Values.Any(x => x.Pattern.HasAssignment()) ||
            (_packPattern?.HasAssignment() ?? false);
    }
}