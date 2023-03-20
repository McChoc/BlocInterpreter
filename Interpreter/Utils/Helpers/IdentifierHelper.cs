using System.Linq;
using Bloc.Utils.Constants;

namespace Bloc.Utils.Helpers;

internal static class IdentifierHelper
{
    internal static bool IsIdentifierValid(string identifier)
    {
        if (identifier.Intersect(Character.ReservedCharacters).Any())
            return false;

        if (char.IsDigit(identifier[0]))
            return false;

        return true;
    }
}