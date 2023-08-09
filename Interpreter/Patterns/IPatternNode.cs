using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Patterns;

public interface IPatternNode
{
    bool Matches(Value value, Call call);
    bool HasAssignment();
}