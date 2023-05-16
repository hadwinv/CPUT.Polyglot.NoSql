using Superpower.Display;

namespace CPUT.Polyglot.NoSql.Common.Parsers
{
    public enum Lexicons
    {
        None = 0,
        Error = 1,

        //keywords
        //DML
        [Token(Category = "Keyword")] FETCH,
        [Token(Category = "Keyword")] MODIFY,
        [Token(Category = "Operator", Example = "+")] ADD,
        //[Token(Category = "Keyword")] INSERT,
        //DDL
        [Token(Category = "Keyword")] ALTER,
        [Token(Category = "Keyword")] CREATER,
        [Token(Category = "Keyword")] DESCRIBE,

        [Token(Category = "Keyword")] PROPERTIES,
        [Token(Category = "Keyword", Example = "Table(n)")] DATA_MODEL,
        [Token(Category = "Keyword", Example = "Table1 = Table2")] LINK_ON,
        [Token(Category = "Keyword")] FILTER_ON,
        [Token(Category = "Keyword")] RESTRICT_TO,
        [Token(Category = "Keyword")] GROUP_BY,
        [Token(Category = "Keyword")] ORDER_BY,
        [Token(Category = "Keyword")] TARGET,
        [Token(Category = "Keyword")] PRECEDENCE,
        //vendor
        [Token(Category = "Vendor")] NAMED_VENDOR,

        [Token(Category = "Keyword")] ON,
        [Token(Category = "Keyword")] AS,
        //schema
        [Token(Category = "Schema", Example = "Column(n)")] PROPERTY,
        [Token(Category = "Schema", Example = "Column(n.json)")] JSON_PROPERTY,
        [Token(Category = "Schema", Example = "Entity(n)")] DATA,
        //condition
        [Token(Category = "Condition")] LH_FILTER_EXP,
        [Token(Category = "Condition")] RH_FILTER_EXP,
        [Token(Category = "Condition")] TERM,
       
        //operators
        [Token(Category = "Operator", Example = "=")] EQL,
        [Token(Category = "Operator", Example = ">=")] GTE,
        [Token(Category = "Operator", Example = ">")] GTR,
        [Token(Category = "Operator", Example = "<")] LSS,
        [Token(Category = "Operator", Example = "<=")] LTE,
        //compare
        [Token(Category = "Operator", Example = "and")] LAND,
        [Token(Category = "Operator", Example = "or")] LOR,

        //aggregators
        [Token] SUM,
        [Token] COUNT,
        [Token] AVG,
        [Token] MIN,
        [Token] MAX,

        [Token] NSUM,
        [Token] NCOUNT,
        [Token] NAVG,
        [Token] NMIN,
        [Token] NMAX,

        //other
        [Token(Example = ",")] COMMA,
        [Token(Example = "(")] LEFT_PAREN,
        [Token(Example = ")")] RIGHT_PAREN,
        [Token(Example = ";")] SEMICOLON,
        [Token(Example = "[")] LEFT_BRACKET,
        [Token(Example = "]")] RIGHT_BRACKET,
        [Token(Example = "{")] LEFT_CURLY_BRACKET,
        [Token(Example = "}")] RIGHT_CURLY_BRACKET,
        [Token(Category = "Punctuation", Example = ".")] DOT,
        [Token] REFERENCE_ALIAS,
        [Token] REFERENCE_ALIAS_NAME,
        [Token(Example = "model as m")] REFERENCE_MODEL,

        //primitive types
        [Token] NUMBER,
        [Token] STRING,

        [Token] ASC,
        [Token] DESC
    }
}
