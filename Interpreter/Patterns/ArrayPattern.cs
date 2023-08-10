using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Patterns;

[Record]
internal sealed partial class ArrayPattern : IPatternNode
{
    private readonly int _packIndex;
    private readonly IPatternNode? _packPattern;
    private readonly List<IPatternNode> _patterns;

    internal ArrayPattern(List<IPatternNode> patterns, IPatternNode? packPattern, int packIndex)
    {
        _patterns = patterns;
        _packPattern = packPattern;
        _packIndex = packIndex;
    }

    public bool Matches(Value value, Call call)
    {
        if (value is not Array array)
            return false;

        if (_packIndex < 0)
        {
            if (array.Values.Count != _patterns.Count)
                return false;

            for (int i = 0; i < _patterns.Count; i++)
            {
                var pattern = _patterns[i];
                var subValue = array.Values[i].Value;

                if (!pattern.Matches(subValue, call))
                    return false;
            }
        }
        else
        {
            int packedCount = array.Values.Count - _patterns.Count;

            if (packedCount < 0)
                return false;

            for (int i = 0; i <= _patterns.Count; i++)
            {
                if (i != _packIndex)
                {
                    var pattern = i < _packIndex
                        ? _patterns[i]
                        : _patterns[i - 1];

                    var subValue = i < _packIndex
                        ? array.Values[i].Value
                        : array.Values[i + packedCount - 1].Value;

                    if (!pattern.Matches(subValue, call))
                        return false;
                }
                else if (_packPattern is not null) 
                {
                    var values = array.Values
                        .GetRange(i, packedCount)
                        .Select(x => x.Value)
                        .ToList();

                    var subValue = new Array(values);

                    if (!_packPattern.Matches(subValue, call))
                        return false;
                }
            }
        }

        return true;
    }

    public bool HasAssignment()
    {
        return _patterns.Any(x => x.HasAssignment()) ||
            (_packPattern?.HasAssignment() ?? false);
    }
}