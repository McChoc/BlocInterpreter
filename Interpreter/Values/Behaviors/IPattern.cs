using Bloc.Patterns;

namespace Bloc.Values.Behaviors;

internal interface IPattern
{
    IPatternNode GetRoot();
}