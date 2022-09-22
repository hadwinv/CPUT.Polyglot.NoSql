using System.Collections.Generic;
using Superpower.Display;

namespace CPUT.Polyglot.NoSql.Parser.QueryBuilder.Lexicals
{
    /// <summary>
    /// Lexical tokens that makes up the unified query expression. 
    /// </summary>
    /// <remarks>
    /// </remarks>
    public enum Lexicon
    {
        None = 0,

        [Token(Example = "=")] EQL,
        [Token(Example = ",")] COMMA,
        [Token(Example = ":")] COLON,
        [Token(Example = "{")] LEFT_BRACE,
        [Token(Example = "[")] LEFT_BRACKET,
        [Token(Example = "(")] LEFT_PAREN,
        [Token(Example = "}")] RIGHT_BRACE,
        [Token(Example = "]")] RIGHT_BRACKET,
        [Token(Example = ")")] RIGHT_PAREN,
        [Token(Example = ";")] SEMICOLON,

        [Token] COMMENT,
        [Token] DURATION,

        //// TODO Don't currentl use error, could be useful for more informative error messages in the parser?
        [Token] ERROR,

        [Token] IDENTIFIER,

        [Token] METRIC_IDENTIFIER,
        [Token] NUMBER,

        [Token] STRING,
        [Token] TIMES,

        // Operators
        [Token(Category = "Operator", Example = "+")] ADD,

        [Token(Category = "Operator", Example = "/")] DIV,

        [Token(Category = "Operator", Example = "==")] EQLC,

        [Token(Category = "Operator", Example = "=~")] EQL_REGEX,

        [Token(Category = "Operator", Example = ">=")] GTE,

        [Token(Category = "Operator", Example = ">")] GTR,

        [Token(Category = "Operator", Example = "and")] LAND,

        [Token(Category = "Operator", Example = "or")] LOR,

        [Token(Category = "Operator", Example = "<")] LSS,

        [Token(Category = "Operator", Example = "<=")] LTE,

        [Token(Category = "Operator", Example = "unless")]
        LUNLESS,

        [Token(Category = "Operator", Example = "%")] MOD,

        [Token(Category = "Operator", Example = "*")] MUL,

        [Token(Category = "Operator", Example = "!=")] NEQ,

        [Token(Category = "Operator", Example = "!~")] NEQ_REGEX,

        [Token(Category = "Operator", Example = "^")] POW,

        [Token(Category = "Operator", Example = "-")] SUB,

        [Token(Category = "Operator", Example = "@")] AT,

        [Token(Category = "Operator", Example = "atan2")] ATAN2,

        // Aggregators
        [Token] AGGREGATE_OP,

        // Keywords
        [Token(Category = "Keyword")] BOOL,

        [Token(Category = "Keyword")] BY,

        [Token(Category = "Keyword")] GROUP_LEFT,

        [Token(Category = "Keyword")] GROUP_RIGHT,

        [Token(Category = "Keyword")] IGNORING,

        [Token(Category = "Keyword")] OFFSET,

        [Token(Category = "Keyword")] ON,

        [Token(Category = "Keyword")] WITHOUT,

        [Token(Category = "Keyword")] FETCH,
        [Token(Category = "Keyword")] MODIFY,
        [Token(Category = "Keyword")] DELETE,
        [Token(Category = "Keyword")] INSERT,
        [Token(Category = "Keyword")] TARGET,
        [Token(Category = "Keyword")] PRECEDENCE,
        [Token(Category = "Keyword")] FILTER_ON,
        [Token(Category = "Keyword")] GROUP_BY,
        [Token(Category = "Keyword")] RESTRICT,
        [Token(Category = "Keyword")] DESCRIBE,
        [Token(Category = "Keyword")] COMBINE


    }
}
