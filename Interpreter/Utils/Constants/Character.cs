using System.Collections.Generic;
using System.Linq;

namespace Bloc.Utils.Constants;

internal static class Character
{
    private const string RESERVED_CHARACTERS = "!\"#$%&'()*+,-./:;<=>?@[\\]^`{|}~";

    internal static IReadOnlyCollection<char> ReservedCharacters { get; }

    static Character()
    {
        ReservedCharacters = RESERVED_CHARACTERS.ToHashSet();
    }
}