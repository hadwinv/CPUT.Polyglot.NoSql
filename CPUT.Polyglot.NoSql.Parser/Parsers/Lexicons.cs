using Superpower.Display;

namespace CPUT.Polyglot.NoSql.Parser.Tokenizers
{
    public enum Lexicons
    {
        None = 0,

        //keywords
        [Token(Category = "Keyword")] FETCH,
        [Token(Category = "Keyword", Example = "Table(n)")] DATA_MODEL,
        [Token(Category = "Keyword", Example = "Table1 = Table2")] LINK_ON,
        [Token(Category = "Keyword")] FILTER_ON,
        [Token(Category = "Keyword")] RESTRICT_TO,
        [Token(Category = "Keyword")] TARGET,
        [Token(Category = "Keyword")] MODIFY,
        [Token(Category = "Keyword")] DELETE,
        [Token(Category = "Keyword")] INSERT,
        [Token(Category = "Keyword")] PRECEDENCE,
        [Token(Category = "Keyword")] GROUP_BY,
        [Token(Category = "Keyword")] DESCRIBE,
        [Token(Category = "Keyword")] COMBINE,
        [Token(Category = "Keyword")] ON,
        [Token(Category = "Keyword")] AS,
        [Token(Category = "Keyword")] PROPERTIES,

        //schema
        [Token(Category = "Schema", Example = "Column(n)")] PROPERTY,
        [Token(Category = "Schema", Example = "Entity(n)")] DATA,
        
        //condition
        [Token(Category = "Condition")] LH_FILTER_EXP,
        [Token(Category = "Condition")] RH_FILTER_EXP,
        [Token(Category = "Condition")] TERM,

        //vendor
        [Token(Category = "Vendor")] NAMED_VENDOR,

        //operators
        [Token(Category = "Operator", Example = "+")] ADD,
        [Token(Category = "Operator", Example = "/")] DIV,
        [Token(Category = "Operator", Example = "=")] EQL,
        [Token(Category = "Operator", Example = ">=")] GTE,
        [Token(Category = "Operator", Example = ">")] GTR,
        [Token(Category = "Operator", Example = "<")] LSS,
        [Token(Category = "Operator", Example = "<=")] LTE,
        [Token(Category = "Operator", Example = "*")] MUL,
        [Token(Category = "Operator", Example = "-")] SUB,
        [Token(Category = "Operator", Example = "and")] LAND,
        [Token(Category = "Operator", Example = "or")] LOR,

        //aggregators
        [Token] SUM,
        [Token] COUNT,
        [Token] AVG,
        [Token] MIN,
        [Token] MAX,

        //other
        [Token(Example = ",")] COMMA,
        [Token(Example = "(")] LEFT_PAREN,
        [Token(Example = ")")] RIGHT_PAREN,
        [Token(Example = ";")] SEMICOLON,
        [Token(Example = "[")] LEFT_BRACKET,
        [Token(Example = "]")] RIGHT_BRACKET,

        [Token] COMMENT,
        [Token] NUMBER,
        [Token] STRING,
        [Token] TIMES,
    }
}
