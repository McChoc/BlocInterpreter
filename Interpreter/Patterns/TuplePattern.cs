using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Utils.Attributes;
using Bloc.Utils.Extensions;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Patterns;

[Record]
internal sealed partial class TuplePattern : IPatternNode
{
    private readonly List<IPatternNode> _patterns;

    internal TuplePattern(List<IPatternNode> patterns)
    {
        _patterns = patterns;
    }

    public bool Matches(Value value, Call call)
    {
        if (value is not Tuple tuple)
            return false;

        if (tuple.Values.Count != _patterns.Count)
            return false;

        foreach (var (element, pattern) in tuple.Values.Zip(_patterns))
            if (!pattern.Matches(element.Value, call))
                return false;
        
        return true;
    }

    public bool HasAssignment()
    {
        return _patterns.Any(x => x.HasAssignment());
    }
}