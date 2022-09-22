using CPUT.Polyglot.NoSql.Parser.Parsers.Operators;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace CPUT.Polyglot.NoSql.Parser.Tokenizers
{
    public class Lexer : Tokenizer<Lexicons>
    {
        protected override IEnumerable<Result<Lexicons>> Tokenize(TextSpan input)
        {
            var next = SkipWhiteSpace(input);

            if (!next.HasValue)
                yield break;

            var bracketsOpen = false;
            var parenDepth = 0;
            var textOpen = false;

            var keywordTracking = new List<string>();
            var binaryOperators = new List<string>();

            var lhs = false;

            do
            {
                var word = string.Empty;
                var symbol = string.Empty;

                if (char.IsLetter(next.Value))
                {
                    var wordStart = next.Location;

                    do
                    {
                        word += next.Value;
                        next = next.Remainder.ConsumeChar();

                    } while (next.HasValue && (char.IsLetter(next.Value) || next.Value == '_'));

                    if (KeywordsToTokens.ContainsKey(word))
                    {
                        if (!(word == "AND") && !(word == "OR"))
                            keywordTracking.Add(word);

                        yield return Result.Value(KeywordsToTokens[word], wordStart, next.Location);
                    }
                    else
                    {
                        if (keywordTracking[keywordTracking.Count - 1] == "FETCH")
                        {
                            yield return Result.Value(PropertyOrFunction.Parse(word), wordStart, next.Location);
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "ADD")
                        {
                            yield return Result.Value(Lexicons.DATA, wordStart, next.Location);
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "MODIFY")
                        {
                            yield return Result.Value(Lexicons.DATA, wordStart, next.Location);
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "PROPERTIES")
                        {
                            if (!lhs)
                            {
                                lhs = true;
                                yield return Result.Value(TermOrFunction.Parse(word), wordStart, next.Location);
                            }
                            else
                            {
                                lhs = false;
                                yield return Result.Value(TermOrFunction.Parse(word), wordStart, next.Location);
                            }
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "DATA_MODEL")
                        {
                            yield return Result.Value(Lexicons.DATA, wordStart, next.Location);
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "LINK_ON")
                        {
                            if (!lhs)
                            {
                                lhs = true;
                                yield return Result.Value(Lexicons.TERM, wordStart, next.Location);
                            }
                            else
                            {
                                lhs = false;
                                yield return Result.Value(Lexicons.TERM, wordStart, next.Location);
                            }
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "FILTER_ON")
                        {
                            if (!lhs)
                            {
                                lhs = true;
                                yield return Result.Value(Lexicons.TERM, wordStart, next.Location);

                            }
                            else
                            {
                                lhs = false;
                                yield return Result.Value(Lexicons.TERM, wordStart, next.Location);
                            }
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "GROUP_BY")
                        {
                            yield return Result.Value(Lexicons.PROPERTY, wordStart, next.Location);
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "TARGET")
                        {
                            yield return Result.Value(Lexicons.NAMED_VENDOR, wordStart, next.Location);
                        }
                        else
                        {
                            yield return Result.Empty<Lexicons>(next.Location, $"unrecognized `{next.Value}`");
                        }
                    }
                }
                else if (char.IsSymbol(next.Value))
                {
                    do
                    {
                        symbol += next.Value;
                        next = next.Remainder.ConsumeChar();

                    } while (next.HasValue && char.IsSymbol(next.Value));

                    if (symbol == "=")
                    {
                        yield return Result.Value(Lexicons.EQL, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else if (symbol == "<")
                    {
                        yield return Result.Value(Lexicons.LSS, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else if (symbol == ">")
                    {
                        yield return Result.Value(Lexicons.GTR, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else if (symbol == ">=")
                    {
                        yield return Result.Value(Lexicons.GTE, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else if (symbol == "<=")
                    {
                        yield return Result.Value(Lexicons.LTE, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<Lexicons>(next.Location, $"unrecognized `{next.Value}`");
                        next = next.Remainder.ConsumeChar(); // Skip the character anyway
                    }
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
                        do
                        {
                            word += next.Value;
                            next = next.Remainder.ConsumeChar();

                            //extract string literal
                            if(next.Value == Convert.ToChar("'"))
                            {
                                word += next.Value;
                                textOpen = false;
                                break;
                            }

                        } while (next.HasValue);

                        if(!textOpen)
                        {
                            yield return Result.Value(Lexicons.STRING, next.Location, next.Remainder);
                        }
                        else
                        {
                            yield return Result.Empty<Lexicons>(next.Location, $"unrecognized `{next.Value}`");
                        }
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


        private static Dictionary<string, Lexicons> KeywordsToTokens = new(StringComparer.OrdinalIgnoreCase)
        {
            ["data_model"] = Lexicons.DATA_MODEL,
            ["fetch"] = Lexicons.FETCH,
            ["modify"] = Lexicons.MODIFY,
            ["add"] = Lexicons.ADD,
            ["target"] = Lexicons.TARGET,
            ["precedence"] = Lexicons.PRECEDENCE,
            ["properties"] = Lexicons.PROPERTIES,
            ["filter_on"] = Lexicons.FILTER_ON,
            ["group_by"] = Lexicons.GROUP_BY,
            ["restrict_to"] = Lexicons.RESTRICT_TO,
            ["link_on"] = Lexicons.LINK_ON,
            ["describe"] = Lexicons.DESCRIBE,
            ["combine"] = Lexicons.COMBINE,
            ["on"] = Lexicons.ON,
            ["as"] = Lexicons.AS,
            // Operands
            ["and"] = Lexicons.LAND,
            ["or"] = Lexicons.LOR

            //["sum"] = Lexicons.SUM,
            //["or"] = Lexicons.AVG
            //["and"] = Lexicons.COUNT,
            //["or"] = Lexicons.MIN,
            //["or"] = Lexicons.MAX
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
