using System;
using System.Collections.Generic;
using System.Reflection;
using Bloc.Utils.Attributes;

namespace Bloc.Utils.Constants;

internal static class Keyword
{
    #region Literal Keywords

    [LiteralKeyword] internal const string VOID = "void";
    [LiteralKeyword] internal const string NULL = "null";
    [LiteralKeyword] internal const string FALSE = "false";
    [LiteralKeyword] internal const string TRUE = "true";
    [LiteralKeyword] internal const string NAN = "nan";
    [LiteralKeyword] internal const string INFINITY = "infinity";

    [LiteralKeyword] internal const string VOID_T = "void_t";
    [LiteralKeyword] internal const string NULL_T = "null_t";
    [LiteralKeyword] internal const string BOOL = "bool";
    [LiteralKeyword] internal const string NUMBER = "number";
    [LiteralKeyword] internal const string RANGE = "range";
    [LiteralKeyword] internal const string STRING = "string";
    [LiteralKeyword] internal const string ARRAY = "array";
    [LiteralKeyword] internal const string STRUCT = "struct";
    [LiteralKeyword] internal const string TUPLE = "tuple";
    [LiteralKeyword] internal const string FUNC = "func";
    [LiteralKeyword] internal const string TASK = "task";
    [LiteralKeyword] internal const string ITER = "iter";
    [LiteralKeyword] internal const string REFERENCE = "reference";
    [LiteralKeyword] internal const string EXTERN = "extern";
    [LiteralKeyword] internal const string TYPE = "type";
    [LiteralKeyword] internal const string PATTERN = "pattern";

    [LiteralKeyword] internal const string ANY = "any";
    [LiteralKeyword] internal const string NONE = "none";

    #endregion

    #region Soft Keywords

    [SoftKeyword] internal const string DISCARD = "_";
    [SoftKeyword] internal const string ASYNC = "async";
    [SoftKeyword] internal const string UNCHECKED = "unchecked";

    #endregion

    #region Hard Keywords

    [HardKeyword] internal const string AS = "as";
    [HardKeyword] internal const string AWAIT = "await";
    [HardKeyword] internal const string BREAK = "break";
    [HardKeyword] internal const string CASE = "case";
    [HardKeyword] internal const string CATCH = "catch";
    [HardKeyword] internal const string CHR = "chr";
    [HardKeyword] internal const string CONST = "const";
    [HardKeyword] internal const string CONTINUE = "continue";
    [HardKeyword] internal const string DEFAULT = "default";
    [HardKeyword] internal const string DELETE = "delete";
    [HardKeyword] internal const string DO = "do";
    [HardKeyword] internal const string ELSE = "else";
    [HardKeyword] internal const string EVAL = "eval";
    [HardKeyword] internal const string EXEC = "exec";
    [HardKeyword] internal const string EXPORT = "export";
    [HardKeyword] internal const string FINALLY = "finally";
    [HardKeyword] internal const string FOR = "for";
    [HardKeyword] internal const string FOREACH = "foreach";
    [HardKeyword] internal const string FROM = "from";
    [HardKeyword] internal const string GLOBAL = "global";
    [HardKeyword] internal const string GOTO = "goto";
    [HardKeyword] internal const string GROUPBY = "groupby";
    [HardKeyword] internal const string IF = "if";
    [HardKeyword] internal const string IMPORT = "import";
    [HardKeyword] internal const string IN = "in";
    [HardKeyword] internal const string IS = "is";
    [HardKeyword] internal const string LEN = "len";
    [HardKeyword] internal const string LET = "let";
    [HardKeyword] internal const string LOCAL = "local";
    [HardKeyword] internal const string LOCK = "lock";
    [HardKeyword] internal const string LOOP = "loop";
    [HardKeyword] internal const string LVAL = "lval";
    [HardKeyword] internal const string NAMEOF = "nameof";
    [HardKeyword] internal const string NEW = "new";
    [HardKeyword] internal const string NEXT = "next";
    [HardKeyword] internal const string NOT = "not";
    [HardKeyword] internal const string ORD = "ord";
    [HardKeyword] internal const string ORDERBY = "orderby";
    [HardKeyword] internal const string OUTER = "outer";
    [HardKeyword] internal const string PARAM = "param";
    [HardKeyword] internal const string REF = "ref";
    [HardKeyword] internal const string REPEAT = "repeat";
    [HardKeyword] internal const string RETURN = "return";
    [HardKeyword] internal const string RVAL = "rval";
    [HardKeyword] internal const string SELECT = "select";
    [HardKeyword] internal const string SWITCH = "switch";
    [HardKeyword] internal const string THROW = "throw";
    [HardKeyword] internal const string TOPLVL = "toplvl";
    [HardKeyword] internal const string TRY = "try";
    [HardKeyword] internal const string TYPEOF = "typeof";
    [HardKeyword] internal const string UNLESS = "unless";
    [HardKeyword] internal const string UNTIL = "until";
    [HardKeyword] internal const string VAL = "val";
    [HardKeyword] internal const string VAR = "var";
    [HardKeyword] internal const string WHEN = "when";
    [HardKeyword] internal const string WHERE = "where";
    [HardKeyword] internal const string WHILE = "while";
    [HardKeyword] internal const string YIELD = "yield";

    #endregion

    #region Composite Keywords

    [CompositeKeyword] internal const string IS_NOT = $"{IS} {NOT}";
    [CompositeKeyword] internal const string NOT_IN = $"{NOT} {IN}";
    [CompositeKeyword] internal const string LET_NEW = $"{LET} {NEW}";
    [CompositeKeyword] internal const string CONST_NEW = $"{CONST} {NEW}";
    [CompositeKeyword] internal const string SELECT_MANY = $"{SELECT} {Symbol.UNPACK_ITER}";

    #endregion

    internal static IReadOnlyCollection<string> LiteralKeywords { get; }
    internal static IReadOnlyCollection<string> HardKeywords { get; }

    static Keyword()
    {
        var literalKeywords = new HashSet<string>();
        var hardKeywords = new HashSet<string>();

        foreach (var field in typeof(Keyword).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (Attribute.IsDefined(field, typeof(LiteralKeywordAttribute)))
                literalKeywords.Add(field.GetValue(null).ToString());
            else if (Attribute.IsDefined(field, typeof(HardKeywordAttribute)))
                hardKeywords.Add(field.GetValue(null).ToString());
        }

        LiteralKeywords = literalKeywords;
        HardKeywords = hardKeywords;
    }
}