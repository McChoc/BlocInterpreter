using Bloc.Memory;

namespace Bloc.Identifiers;

internal interface INamedIdentifier : IIdentifier
{
    string GetName(Call call);
}