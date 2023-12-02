using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bloc.Utils.Constants;

internal static class Symbol
{
    #region Symbols

    internal const string PLUS = "+";
    internal const string PLUS_EQ = "+=";
    internal const string DBL_PLUS = "++";
    internal const string MINUS = "-";
    internal const string MINUS_EQ = "-=";
    internal const string DBL_MINUS = "--";
    internal const string STAR = "*";
    internal const string STAR_EQ = "*=";
    internal const string DBL_STAR = "**";
    internal const string DBL_STAR_EQ = "**=";
    internal const string SLASH = "/";
    internal const string SLASH_EQ = "/=";
    internal const string PERCENT = "%";
    internal const string PERCENT_EQ = "%=";
    internal const string DBL_PERCENT = "%%";
    internal const string DBL_PERCENT_EQ = "%%=";
    internal const string TILDE = "~";
    internal const string DBL_TILDE = "~~";
    internal const string EXCL = "!";
    internal const string DBL_EXCL = "!!";
    internal const string AMP = "&";
    internal const string AMP_EQ = "&=";
    internal const string DBL_AMP = "&&";
    internal const string DBL_AMP_EQ = "&&=";
    internal const string BAR = "|";
    internal const string BAR_EQ = "|=";
    internal const string DBL_BAR = "||";
    internal const string DBL_BAR_EQ = "||=";
    internal const string FLEX = "^";
    internal const string FLEX_EQ = "^=";
    internal const string DBL_FLEX = "^^";
    internal const string DBL_FLEX_EQ = "^^=";
    internal const string L_SHIFT = "<<";
    internal const string L_SHIFT_EQ = "<<=";
    internal const string R_SHIFT = ">>";
    internal const string R_SHIFT_EQ = ">>=";
    internal const string QUESTION = "?";
    internal const string DBL_QUESTION = "??";
    internal const string DBL_QUESTION_EQ = "??=";
    internal const string TPL_QUESTION = "???";
    internal const string MATCH_DECLARE = "->";
    internal const string MATCH_ASSIGN = "->>";
    internal const string EQUAL = "=";
    internal const string DBL_EQ = "==";
    internal const string NOT_EQ_0 = "<>";
    internal const string NOT_EQ_1 = "!=";
    internal const string NOT_EQ_2 = "~=";
    internal const string LESS = "<";
    internal const string LESS_EQ = "<=";
    internal const string MORE = ">";
    internal const string MORE_EQ = ">=";
    internal const string UFO = "<=>";
    internal const string ARROW = "=>";
    internal const string PIPE = "|>";
    internal const string AT = "@";
    internal const string DOLAR = "$";
    internal const string DOT = ".";
    internal const string COMMA = ",";
    internal const string SEMI = ";";
    internal const string COLON = ":";
    internal const string DBL_COLON = "::";
    internal const string RANGE_INC_INC = "..";
    internal const string RANGE_INC_EXC = "..<";
    internal const string RANGE_EXC_INC = ">..";
    internal const string RANGE_EXC_EXC = ">..<";
    internal const string COMMENT = "//";
    internal const string L_COMMENT = "/*";
    internal const string R_COMMENT = "*/";
    internal const string L_PAREN = "(";
    internal const string R_PAREN = ")";
    internal const string L_BRACKET = "[";
    internal const string R_BRACKET = "]";
    internal const string L_BRACE = "{";
    internal const string R_BRACE = "}";
    internal const string BACK_QUOTE = "`";
    internal const string SINGLE_QUOTE = "\'";
    internal const string DOUBLE_QUOTE = "\"";
    internal const string BACK_SLASH = "\\";

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