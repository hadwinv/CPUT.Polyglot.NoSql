using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Parser.Parsers.Operators;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.Collections.Immutable;

namespace CPUT.Polyglot.NoSql.Parser.Tokenizers
{
    public class Lexer : Tokenizer<Lexicons>
    {
        protected override IEnumerable<Result<Lexicons>> Tokenize(TextSpan input)
        {
            var next = SkipWhiteSpace(input);

            if (!next.HasValue)
                yield break;

            var keywordTracking = new List<string>();
            var bracketsOpen = false;
            var parenDepth = 0;
            var textOpen = false;
            var lhs = false;

            do
            {
                var text = string.Empty;

                if (char.IsLetter(next.Value))
                {
                    var wordStart = next.Location;

                    do
                    {
                        text += next.Value;
                        next = next.Remainder.ConsumeChar();

                    } while (next.HasValue && (char.IsLetter(next.Value) || next.Value == '_') || char.IsDigit(next.Value));

                    if (Keywords.ContainsKey(text))
                    {
                        if (!(text == "AND") && !(text == "OR"))
                            keywordTracking.Add(text);

                        yield return Result.Value(Keywords[text], wordStart, next.Location);
                    }
                    else
                    {
                        if (keywordTracking[keywordTracking.Count - 1] == "FETCH")
                            yield return Result.Value(PropertyOrFunction.Parse(text), wordStart, next.Location);
                        else if (keywordTracking[keywordTracking.Count - 1] == "ADD" 
                            || keywordTracking[keywordTracking.Count - 1] == "MODIFY"
                            || keywordTracking[keywordTracking.Count - 1] == "DATA_MODEL")
                            yield return Result.Value(Lexicons.DATA, wordStart, next.Location);
                        else if (keywordTracking[keywordTracking.Count - 1] == "PROPERTIES" 
                            || keywordTracking[keywordTracking.Count - 1] == "LINK_ON"
                            || keywordTracking[keywordTracking.Count - 1] == "FILTER_ON")
                        {
                            if (!lhs)
                                lhs = true;
                            else
                                lhs = false;

                            yield return Result.Value(TermOrFunction.Parse(text), wordStart, next.Location);
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "GROUP_BY")
                            yield return Result.Value(Lexicons.PROPERTY, wordStart, next.Location);
                        else if (keywordTracking[keywordTracking.Count - 1] == "ORDER_BY")
                            yield return Result.Value(Lexicons.PROPERTY, wordStart, next.Location);
                        else if (keywordTracking[keywordTracking.Count - 1] == "TARGET")
                            yield return Result.Value(Lexicons.NAMED_VENDOR, wordStart, next.Location);
                        else
                            yield return Result.Empty<Lexicons>(next.Location, $"unrecognized `{next.Value}`");
                    }
                }
                else if (char.IsSymbol(next.Value))
                {
                    var wordStart = next.Location;

                    do
                    {
                        text += next.Value;
                        next = next.Remainder.ConsumeChar();

                    } while (next.HasValue && char.IsSymbol(next.Value));

                    if (text == "=")
                        yield return Result.Value(Lexicons.EQL, wordStart, next.Location);
                    else if (text == "<")
                        yield return Result.Value(Lexicons.LSS, next.Location, next.Remainder);
                    else if (text == ">")
                        yield return Result.Value(Lexicons.GTR, next.Location, next.Remainder);
                    else if (text == ">=")
                        yield return Result.Value(Lexicons.GTE, next.Location, next.Remainder);
                    else if (text == "<=")
                        yield return Result.Value(Lexicons.LTE, next.Location, next.Remainder);
                    else
                        yield return Result.Empty<Lexicons>(next.Location, $"unrecognized `{next.Value}`");

                    next = next.Remainder.ConsumeChar(); // Skip the character anyway
                }
                else if (next.Value == '{')
                {
                    if (bracketsOpen)
                        yield return Result.Empty<Lexicons>(next.Location, "Unexpected left bracket");

                    bracketsOpen = true;

                    next = next.Remainder.ConsumeChar();
                }
                else if (next.Value == '}')
                {
                    if (!bracketsOpen)
                        yield return Result.Empty<Lexicons>(next.Location, "Unexpected right bracket");

                    bracketsOpen = false;

                    next = next.Remainder.ConsumeChar();
                }
                else if (next.Value == '(')
                {
                    parenDepth++;
                    yield return Result.Value(Lexicons.LEFT_PAREN, next.Location, next.Remainder);

                    next = next.Remainder.ConsumeChar();
                }
                else if (next.Value == ')')
                {
                    parenDepth--;
                    if (parenDepth < 0)
                        yield return Result.Empty<Lexicons>(next.Location, "Unexpected right parenthesis");

                    yield return Result.Value(Lexicons.RIGHT_PAREN, next.Location, next.Remainder);

                    next = next.Remainder.ConsumeChar();
                }
                else if (next.Value == ',')
                {
                    yield return Result.Value(Lexicons.COMMA, next.Location, next.Remainder);
                    next = next.Remainder.ConsumeChar();
                }
                else
                {
                    if (char.IsDigit(next.Value))
                    {
                        if (lhs)
                            lhs = false;

                        var integer = Numerics.Integer(next.Location);
                        yield return Result.Value(Lexicons.NUMBER, next.Location, integer.Remainder);
                        next = integer.Remainder.ConsumeChar();
                    }
                    else if (next.Value == Convert.ToChar("'"))
                    {
                        if (lhs)
                            lhs = false;

                        textOpen = true;
                        //word += next.Value;
                        next = next.Remainder.ConsumeChar();

                        var wordStart = next.Location;

                        do
                        {
                            next = next.Remainder.ConsumeChar();
                            
                            if (next.Value == Convert.ToChar("'"))
                            {
                                textOpen = false;
                                break;
                            }

                        } while (next.HasValue);

                        if (!textOpen)
                            yield return Result.Value(Lexicons.STRING, wordStart, next.Location);
                        else
                            yield return Result.Empty<Lexicons>(next.Location, $"unrecognized `{next.Value}`");
                        
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<Lexicons>(next.Location, $"unrecognized `{next.Value}`");
                        next = next.Remainder.ConsumeChar(); // Skip the character anyway
                    }
                }

                next = SkipWhiteSpace(next.Location);

            } while (next.HasValue);
        }

        private TextParser<Lexicons> PropertyOrFunction { get; set; } = Span.MatchedBy(
           Character.Letter.Or(Character.In('_', ':')).IgnoreThen(Character.LetterOrDigit.Or(Character.In('_', ':')).Many()))
            .Select(x =>
            {
                var idOrKeyword = x.ToStringValue().ToLower();

                if (Aggregates.ContainsKey(idOrKeyword))
                {
                    if(idOrKeyword == "sum")
                     return Lexicons.SUM;
                    else if (idOrKeyword == "avg")
                        return Lexicons.AVG;
                    else if (idOrKeyword == "count")
                        return Lexicons.COUNT;
                    else if (idOrKeyword == "min")
                        return Lexicons.MIN;
                    else if (idOrKeyword == "max")
                        return Lexicons.MAX;
                }

                return Lexicons.PROPERTY;
            });

        private TextParser<Lexicons> TermOrFunction { get; set; } = Span.MatchedBy(
         Character.Letter.Or(Character.In('_', ':')).IgnoreThen(Character.LetterOrDigit.Or(Character.In('_', ':')).Many()))
          .Select(x =>
          {
              var idOrKeyword = x.ToStringValue().ToLower();

              if (Aggregates.ContainsKey(idOrKeyword))
              {
                  if (idOrKeyword == "sum")
                      return Lexicons.SUM;
                  else if (idOrKeyword == "avg")
                      return Lexicons.AVG;
                  else if (idOrKeyword == "count")
                      return Lexicons.COUNT;
                  else if (idOrKeyword == "min")
                      return Lexicons.MIN;
                  else if (idOrKeyword == "max")
                      return Lexicons.MAX;
              }

              return Lexicons.TERM;
          });

        private static Dictionary<string, Lexicons> Keywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["describe"] = Lexicons.DESCRIBE,
            ["fetch"] = Lexicons.FETCH,
            ["modify"] = Lexicons.MODIFY,
            ["add"] = Lexicons.ADD,
            ["properties"] = Lexicons.PROPERTIES,
            ["data_model"] = Lexicons.DATA_MODEL,
            ["link_on"] = Lexicons.LINK_ON,
            ["filter_on"] = Lexicons.FILTER_ON,
            ["group_by"] = Lexicons.GROUP_BY,
            ["order_by"] = Lexicons.ORDER_BY,
            ["asc"] = Lexicons.ASC,
            ["desc"] = Lexicons.DESC,
            ["restrict_to"] = Lexicons.RESTRICT_TO,
            ["target"] = Lexicons.TARGET,
            ["precedence"] = Lexicons.PRECEDENCE,
            ["and"] = Lexicons.LAND,
            ["or"] = Lexicons.LOR
        };

        private static ImmutableDictionary<string, AggregateOperator> Aggregates { get; set; } = new[]
        {
            new AggregateOperator("sum"),
            new AggregateOperator("avg"),
            new AggregateOperator("count"),
            new AggregateOperator("min"),
            new AggregateOperator("max"),
        }.ToImmutableDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);
    }
}
