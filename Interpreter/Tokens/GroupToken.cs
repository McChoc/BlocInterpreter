﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Bloc.Tokens;

internal sealed class GroupToken : Token
{
    public List<Token> Tokens { get; }

    internal GroupToken(int start, int end, List<Token> tokens)
        : base(start, end)
    {
        Tokens = tokens;
    }

    public sealed override int GetHashCode()
    {
        return HashCode.Combine(Tokens.Count);
    }

    public sealed override bool Equals(object other)
    {
        return other is GroupToken token &&
            Tokens.SequenceEqual(token.Tokens);
    }
}