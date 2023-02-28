using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bloc.Constants;

internal static class Symbol
{
    #region Symbols

    internal const string PLUS = "+";
    internal const string MINUS = "-";
    internal const string TIMES = "*";
    internal const string SLASH = "/";
    internal const string REMAINDER = "%";
    internal const string POWER = "**";
    internal const string ROOT = "//";
    internal const string LOGARITHM = "%%";
    internal const string INCREMENT = "++";
    internal const string DECREMENT = "--";
    internal const string SHIFT_L = "<<";
    internal const string SHIFT_R = ">>";
    internal const string BIT_NOT = "~";
    internal const string BIT_AND = "&";
    internal const string BIT_OR = "|";
    internal const string BIT_XOR = "^";
    internal const string BIT_INV = "~~";
    internal const string BOOL_NOT = "!";
    internal const string BOOL_AND = "&&";
    internal const string BOOL_OR = "||";
    internal const string BOOL_XOR = "^^";
    internal const string BOOL_INV = "!!";
    internal const string COALESCE_NULL = "??";
    internal const string COALESCE_VOID = "???";
    internal const string ASSIGN = "=";
    internal const string ASSIGN_SUM = "+=";
    internal const string ASSIGN_DIF = "-=";
    internal const string ASSIGN_PRODUCT = "*=";
    internal const string ASSIGN_QUOTIENT = "/=";
    internal const string ASSIGN_REMAINDER = "%=";
    internal const string ASSIGN_POWER = "**=";
    internal const string ASSIGN_ROOT = "//=";
    internal const string ASSIGN_LOGARITHM = "%%=";
    internal const string ASSIGN_SHIFT_L = "<<=";
    internal const string ASSIGN_SHIFT_R = ">>=";
    internal const string ASSIGN_BIT_AND = "&=";
    internal const string ASSIGN_BIT_OR = "|=";
    internal const string ASSIGN_BIT_XOR = "^=";
    internal const string ASSIGN_BOOL_AND = "&&=";
    internal const string ASSIGN_BOOL_OR = "||=";
    internal const string ASSIGN_BOOL_XOR = "^^=";
    internal const string ASSIGN_COALESCE = "??=";
    internal const string IS_EQUAL = "==";
    internal const string NOT_EQUAL_0 = "<>";
    internal const string NOT_EQUAL_1 = "!=";
    internal const string NOT_EQUAL_2 = "~=";
    internal const string LESS_THAN = "<";
    internal const string LESS_EQUAL = "<=";
    internal const string MORE_THAN = ">";
    internal const string MORE_EQUAL = ">=";
    internal const string COMPARISON = "<=>";
    internal const string ACCESS_MEMBER = ".";
    internal const string UNPACK_ARRAY = "..";
    internal const string UNPACK_STRUCT = "...";
    internal const string COMMA = ",";
    internal const string COLON = ":";
    internal const string SEMICOLON = ";";
    internal const string QUESTION = "?";
    internal const string LAMBDA = "=>";
    internal const string PIPE = "|>";
    internal const string VARIABLE = "$";
    internal const string ATTRIBUTE = "@";
    internal const string COMMENT = "#";
    internal const string COMMENT_L = "#*";
    internal const string COMMENT_R = "*#";
    internal const string PAREN_L = "(";
    internal const string PAREN_R = ")";
    internal const string BRACKET_L = "[";
    internal const string BRACKET_R = "]";
    internal const string BRACE_L = "{";
    internal const string BRACE_R = "}";
    internal const string SINGLE_QUOTE = "'";
    internal const string DOUBLE_QUOTE = "\"";
    internal const string BACK_QUOTE = "`";
    internal const string ESCAPE_CHAR = "\\";

    #endregion

    internal static IReadOnlyCollection<string> Symbols { get; }

    static Symbol()
    {
        Symbols = typeof(Symbol)
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(string))
            .Select(x => (string)x.GetValue(null))
            .ToHashSet();
    }
}