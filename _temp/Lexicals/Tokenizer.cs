using System;
using System.Collections.Generic;
using System.Linq;
using CPUT.Polyglot.NoSql.Parser.Builder.Lexicals;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace CPUT.Polyglot.NoSql.Parser.QueryBuilder.Lexicals
{
    /// <summary>
    /// Converts streams of characters into <see cref="Lexicon"/>s. These tokens are then consumed by the <see cref="Parser"/>
    /// to build up the abstract syntax tree.
    /// </summary>
    public class Tokenizer : Tokenizer<Lexicon>
    {
        private static Dictionary<string, Lexicon> KeywordsToTokens = new(StringComparer.OrdinalIgnoreCase)
        {
            // Operators
            ["and"] = Lexicon.LAND,
            ["or"] = Lexicon.LOR,
            //["unless"] = Lexicon.LUNLESS,
            //["atan2"] = Lexicon.ATAN2,

            // Keywords
            ["offset"] = Lexicon.OFFSET,
            ["by"] = Lexicon.BY,
            ["without"] = Lexicon.WITHOUT,
            ["on"] = Lexicon.ON,
            ["ignoring"] = Lexicon.IGNORING,
            ["group_left"] = Lexicon.GROUP_LEFT,
            ["group_right"] = Lexicon.GROUP_RIGHT,
            ["bool"] = Lexicon.BOOL,
            ["inf"] = Lexicon.NUMBER,
            ["nan"] = Lexicon.NUMBER,

            ["fetch"] = Lexicon.FETCH,
            ["modify"] = Lexicon.MODIFY,
            ["delete"] = Lexicon.DELETE,
            ["insert"] = Lexicon.INSERT,
            ["target"] = Lexicon.TARGET,
            ["precedence"] = Lexicon.PRECEDENCE,
            ["filter_on"] = Lexicon.FILTER_ON,
            ["group_by"] = Lexicon.GROUP_BY,
            ["restrict"] = Lexicon.RESTRICT,
            ["describe"] = Lexicon.DESCRIBE,
            ["combine"] = Lexicon.COMBINE,


            // TODO support preprocessors
            // Preprocessors
            //["start"] =  PromQlToken.START,
            //["end"] =    PromQlToken.END,
        };

        public TextParser<string> Comment { get; set; } =
            from start in Character.EqualTo('#')
            from rest in Character.ExceptIn('\r', '\n').Many()
            select new string(rest);

        public TextParser<string> Number { get; set; } =
            from integer in Character.Numeric.Many()
            from dec in Character.EqualTo('.').Optional()
            from fraction in Character.Numeric.Many()
            from exponentPart in (
                from expontentDelim in Character.In('e', 'E')
                from sign in Character.In('+', '-').Optional()
                from exponent in Character.Numeric.AtLeastOnce()
                select exponent
            ).OptionalOrDefault()
            where integer.Length >= 1 || fraction.Length >= 1
            select new string(integer);

        public TextParser<string> Duration { get; set; } =
            from d in (
                from num in Character.Numeric.AtLeastOnce()
                from unit in Character.In('s', 'm', 'h', 'd', 'w', 'y').AtLeastOnce()
                select new string(num)
            ).AtLeastOnce()
            select new string(d.SelectMany(x => x).ToArray());

        public static TextParser<Unit> QuotedSting(char quoteChar, bool lineBreakAllowed = false) =>
            from open in Character.EqualTo(quoteChar)
            from content in
                    Character.EqualTo('\\').IgnoreThen(Character.AnyChar).Try().Or(Character.ExceptIn(
                        new[] { quoteChar }.Concat(lineBreakAllowed ? Array.Empty<char>() : new[] { '\n' }).ToArray()))

                .Many()
            from close in Character.EqualTo(quoteChar)
            select Unit.Value;

        public TextParser<Lexicon> Identifier { get; set; } = Span.MatchedBy(
                Character.Letter.Or(Character.In('_')).IgnoreThen(Character.LetterOrDigit.Or(Character.In('_')).Many())
            )
            .Select(x => Lexicon.IDENTIFIER);

        public TextParser<Lexicon> IndentifierOrKeyword { get; set; } = Span.MatchedBy(
            Character.Letter.Or(Character.In('_', ':')).IgnoreThen(Character.LetterOrDigit.Or(Character.In('_', ':')).Many())
            )
            .Select(x =>
            {
                var idOrKeyword = x.ToStringValue();
                if (Operators11.Aggregates.ContainsKey(idOrKeyword))
                    return Lexicon.AGGREGATE_OP;
                if (KeywordsToTokens.TryGetValue(idOrKeyword, out var keyToken))
                    return keyToken;
                if (idOrKeyword.Contains(":"))
                    return Lexicon.METRIC_IDENTIFIER;

                return Lexicon.IDENTIFIER;
            });

        public TextParser<Lexicon> String { get; set; } = QuotedSting('\'')
            .Or(QuotedSting('"'))
            .Or(QuotedSting('`', lineBreakAllowed: true))
            .Select(_ => Lexicon.STRING);

        public class Reader
        {
            private Result<char> _start;

            public Reader(TextSpan input)
            {
                Input = input;
                SkipWhiteSpace(input);
            }

            public TextSpan Input { get; }
            public Result<char> Position { get; private set; }

            public Result<char> Peek() => Position.Remainder.ConsumeChar();

            public bool Next()
            {
                Position = Position.Remainder.ConsumeChar();
                return Position.Remainder.IsAtEnd;
            }

            public bool TryParse<TParser>(TextParser<TParser> parser, out Result<TParser> result)
            {
                result = parser(Position.Location);

                if (result.HasValue)
                {
                    Position = result.Remainder.ConsumeChar();
                    _start = Position;
                    return true;
                }

                return false;
            }

            public bool TryParseToken<TParser>(TextParser<TParser> parser, Lexicon promToken, out Result<Lexicon> result)
            {
                result = default;

                if (!TryParse(parser, out var pResult))
                    return false;

                result = Result.Value(promToken, pResult.Location, pResult.Remainder);
                return true;
            }

            public bool TryParseToken(TextParser<Lexicon> parser, out Result<Lexicon> result)
            {
                result = default;

                if (!TryParse(parser, out var pResult))
                    return false;

                result = Result.Value(pResult.Value, pResult.Location, pResult.Remainder);
                return true;
            }

            public Result<Lexicon> AsToken(Lexicon promToken)
            {
                var r = Result.Value(promToken, _start.Location, Position.Remainder);
                Next();
                _start = Position;
                return r;
            }

            public void SkipWhiteSpace()
            {
                SkipWhiteSpace(Position.Location);
            }

            private void SkipWhiteSpace(TextSpan span)
            {
                var result = span.ConsumeChar();
                while (result.HasValue && char.IsWhiteSpace(result.Value))
                    result = result.Remainder.ConsumeChar();

                _start = Position = result;
            }

            public Result<Lexicon> AsError(string errMsg)
            {
                return Result.Empty<Lexicon>(Position.Remainder, errMsg);
            }
        }

        protected override IEnumerable<Result<Lexicon>> Tokenize(TextSpan span)
        {
            var reader = new Reader(span);
            if (!reader.Position.HasValue)
                yield break;

            var bracketsOpen = false;
            var parenDepth = 0;

            do
            {
                Result<Lexicon> token = default;
                var c = reader.Position.Value;

                // TODO brace open + comment are lexed separately from main body in PromQl lexer, is this an issue?
                if (c == '{')
                {
                    yield return reader.AsToken(Lexicon.LEFT_BRACE);
                    foreach (var t in TokenizeInsideBraces(reader))
                        yield return t;
                }
                else if (reader.TryParseToken(Comment, Lexicon.COMMENT, out token))
                    yield return reader.AsToken(Lexicon.COMMENT);
                else if (c == ',')
                    yield return reader.AsToken(Lexicon.COMMA);
                else if (c == ',')
                    yield return reader.AsToken(Lexicon.COMMA);
                else if (c == '*')
                    yield return reader.AsToken(Lexicon.MUL);
                else if (c == '/')
                    yield return reader.AsToken(Lexicon.DIV);
                else if (c == '%')
                    yield return reader.AsToken(Lexicon.MOD);
                else if (c == '+')
                    yield return reader.AsToken(Lexicon.ADD);
                else if (c == '-')
                    yield return reader.AsToken(Lexicon.SUB);
                else if (c == '^')
                    yield return reader.AsToken(Lexicon.POW);
                else if (c == '=')
                {
                    var n = reader.Peek();
                    if (n.Value == '=')
                    {
                        reader.Next();
                        yield return reader.AsToken(Lexicon.EQLC);
                    }
                    // TODO missing err condition
                    else
                        yield return reader.AsToken(Lexicon.EQL);
                }
                else if (c == '!')
                {
                    var n = reader.Peek();
                    if (n.Value == '=')
                    {
                        reader.Next();
                        yield return reader.AsToken(Lexicon.NEQ);
                    }
                    else
                        yield return reader.AsError("Unexpected character after !");
                }
                else if (c == '<')
                {
                    var n = reader.Peek();
                    if (n.Value == '=')
                    {
                        reader.Next();
                        yield return reader.AsToken(Lexicon.LTE);
                    }
                    else
                        yield return reader.AsToken(Lexicon.LSS);
                }
                else if (c == '>')
                {
                    var n = reader.Peek();
                    if (n.Value == '=')
                    {
                        reader.Next();
                        yield return reader.AsToken(Lexicon.GTE);
                    }
                    else
                        yield return reader.AsToken(Lexicon.GTR);
                }
                else if (reader.TryParseToken(Duration, Lexicon.DURATION, out token))
                    yield return token;
                else if (reader.TryParseToken(Number, Lexicon.NUMBER, out token))
                    yield return token;
                else if (reader.TryParseToken(String, out token))
                    yield return token;
                else if (bracketsOpen && c == ':')
                    yield return reader.AsToken(Lexicon.COLON);
                else if (reader.TryParse(IndentifierOrKeyword, out token))
                    yield return token;
                // TODO add support for 'at'
                else if (c == '(')
                {
                    parenDepth++;
                    yield return reader.AsToken(Lexicon.LEFT_PAREN);
                }
                else if (c == ')')
                {
                    parenDepth--;
                    if (parenDepth < 0)
                        yield return reader.AsError("Unexpected right parenthesis");

                    yield return reader.AsToken(Lexicon.RIGHT_PAREN);
                }
                else if (c == '[')
                {
                    if (bracketsOpen)
                        yield return reader.AsError("Unexpected left bracket");

                    bracketsOpen = true;
                    yield return reader.AsToken(Lexicon.LEFT_BRACKET);
                }
                else if (c == ']')
                {
                    if (!bracketsOpen)
                        yield return reader.AsError("Unexpected right bracket");

                    bracketsOpen = false;
                    yield return reader.AsToken(Lexicon.RIGHT_BRACKET);
                }
                else
                    yield return Result.Empty<Lexicon>(reader.Position.Remainder);

                reader.SkipWhiteSpace();

            } while (reader.Position.HasValue);

            if (parenDepth != 0)
                yield return reader.AsError("Unclosed left parenthesis");
            else if (bracketsOpen)
                yield return reader.AsError("Unclosed left bracket");
        }

        /// <summary>
        /// Scans inside of a vector selector. Keywords are ignored and scanned as identifiers.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private IEnumerable<Result<Lexicon>> TokenizeInsideBraces(Reader reader)
        {
            Result<Lexicon> token;

            while (true)
            {
                reader.SkipWhiteSpace();
                if (!reader.Position.HasValue)
                {
                    yield return reader.AsError("Unexpected end of input inside braces");
                    yield break;
                }

                var c = reader.Position.Value;

                if (c == '{')
                    yield return reader.AsError("Unexpected left brace");
                else if (c == '}')
                {
                    yield return reader.AsToken(Lexicon.RIGHT_BRACE);
                    yield break;
                }
                else if (reader.TryParseToken(Comment, Lexicon.COMMENT, out token))
                    yield return token;
                else if (reader.TryParseToken(Identifier, out token))
                    yield return token;
                else if (reader.TryParseToken(String, out token))
                    yield return token;
                else if (reader.Position.Value == ',')
                    yield return reader.AsToken(Lexicon.COMMA);
                else if (c == '=')
                {
                    if (reader.Peek().Value == '~')
                    {
                        reader.Next();
                        yield return reader.AsToken(Lexicon.EQL_REGEX);
                    }
                    else
                        yield return reader.AsToken(Lexicon.EQL);
                }
                else if (c == '!')
                {
                    reader.Next();
                    yield return reader.Position.Value switch
                    {
                        '=' => reader.AsToken(Lexicon.NEQ),
                        '~' => reader.AsToken(Lexicon.NEQ_REGEX),
                        _ => reader.AsError($"Unexpected character after ! inside braces: '{reader.Position.Value}'")
                    };
                }
                else
                    yield return reader.AsError($"Unexpected character '{c}' inside braces");
            }
        }
    }
}
