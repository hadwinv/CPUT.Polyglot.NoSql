using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Parser.Parsers.Operators;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace CPUT.Polyglot.NoSql.Parser.Tokenizers
{
    public class Lexer : Tokenizer<Lexicons>
    {

        string WordInBetween(string sentence, string wordOne, string wordTwo)
        {

            int start = sentence.IndexOf(wordOne) + wordOne.Length + 1;

            int end = sentence.IndexOf(wordTwo) - start - 1;

            return sentence.Substring(start, end);


        }

        protected override IEnumerable<Result<Lexicons>> Tokenize(TextSpan input)
        {
            var next = SkipWhiteSpace(input);

            if (!next.HasValue)
                yield break;

            var keywordTracking = new List<string>();
            var outerBracketsOpen = false;
            var squareBracketsOpen = false;
            var innerBracketsOpen = false;
            var parenDepth = 0;
            var textOpen = false;
            var lhs = false;
            var previousText = string.Empty;

            var referenceAdded = false;
            var modelReference = false;
            do
            {
                var text = string.Empty;


                var regex = new Regex(@".*DATA_MODEL(.*)}.*", RegexOptions.IgnorePatternWhitespace);
                if (regex.IsMatch(input.Source.ToString()))
                {
                    var data = regex.Match(input.Source.ToString()).Groups[1].Value;

                    if (data.ToLower().Contains("as"))
                        modelReference = true;
                }

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
                        {
                            if (next.Value == '.')
                            {
                                if(modelReference && !referenceAdded)
                                {
                                    yield return Result.Value(Lexicons.REFERENCE_ALIAS, wordStart, next.Location);
                                    
                                    referenceAdded = true;
                                }
                                else
                                {
                                    if(next.Value == '.')
                                    {
                                        do
                                        {
                                            next = next.Remainder.ConsumeChar();
                                        } while (next.Value == '.' || (next.HasValue && (char.IsLetter(next.Value) || next.Value == '_')));

                                        yield return Result.Value(Lexicons.JSON_PROPERTY, wordStart, next.Location);
                                    }
                                    else
                                        yield return Result.Value(PropertyOrFunction.Parse(text), wordStart, next.Location);

                                    referenceAdded = false;
                                }
                            }
                            else
                            {
                                if (text == "AS")
                                    yield return Result.Value(Lexicons.AS, wordStart, next.Location);
                                else if (previousText == "AS")
                                {
                                    yield return Result.Value(Lexicons.REFERENCE_ALIAS_NAME, wordStart, next.Location);

                                    previousText = string.Empty;
                                }
                                else
                                    yield return Result.Value(PropertyOrFunction.Parse(text), wordStart, next.Location);
                                
                                referenceAdded = false;
                            }

                            previousText = text;
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "ADD" 
                            || keywordTracking[keywordTracking.Count - 1] == "MODIFY"
                            || keywordTracking[keywordTracking.Count - 1] == "DATA_MODEL")
                        {

                            if (text == "AS")
                                yield return Result.Value(Lexicons.AS, wordStart, next.Location);
                            else if (previousText == "AS")
                            {
                                yield return Result.Value(Lexicons.REFERENCE_MODEL, wordStart, next.Location);

                                previousText = string.Empty;
                            }
                            else
                                yield return Result.Value(Lexicons.DATA, wordStart, next.Location);

                            previousText = text;
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "PROPERTIES" 
                            || keywordTracking[keywordTracking.Count - 1] == "FILTER_ON")
                        {
                            if (!lhs)
                                lhs = true;

                            if (next.Value == '.')
                            {
                                if (modelReference && !referenceAdded)
                                {
                                    yield return Result.Value(Lexicons.REFERENCE_ALIAS, wordStart, next.Location);

                                    referenceAdded = true;
                                }
                                else
                                {
                                    if (next.Value == '.')
                                    {
                                        do
                                        {
                                            next = next.Remainder.ConsumeChar();
                                        } while (next.Value == '.' || (next.HasValue && (char.IsLetter(next.Value) || next.Value == '_')));

                                        yield return Result.Value(Lexicons.JSON_PROPERTY, wordStart, next.Location);
                                    }
                                    else
                                        yield return Result.Value(PropertyOrFunction.Parse(text), wordStart, next.Location);

                                    referenceAdded = false;
                                }
                            }
                            else
                            {
                                if (next.Value == '.')
                                    yield return Result.Value(Lexicons.REFERENCE_ALIAS, wordStart, next.Location);
                                else if (text == "AS")
                                    yield return Result.Value(Lexicons.AS, wordStart, next.Location);
                                else if (previousText == "AS")
                                {
                                    yield return Result.Value(Lexicons.REFERENCE_ALIAS_NAME, wordStart, next.Location);

                                    previousText = string.Empty;
                                }
                                else
                                {
                                    if (referenceAdded)
                                        referenceAdded = false;

                                    yield return Result.Value(TermOrFunction.Parse(text), wordStart, next.Location);
                                }
                            }
                           
                            previousText = text;
                        }
                        else if (keywordTracking[keywordTracking.Count - 1] == "ORDER_BY")
                        {
                            if (next.Value == '.')
                                yield return Result.Value(Lexicons.REFERENCE_ALIAS, wordStart, next.Location);
                            else if (text == "AS")
                                yield return Result.Value(Lexicons.AS, wordStart, next.Location);
                            else if (previousText == "AS")
                            {
                                yield return Result.Value(Lexicons.REFERENCE_ALIAS_NAME, wordStart, next.Location);

                                previousText = string.Empty;
                            }
                            else
                                yield return Result.Value(Lexicons.PROPERTY, wordStart, next.Location);

                            previousText = text;
                        }
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

                    if (lhs)
                        lhs = false;
                }
                else if (next.Value == '{')
                {
                    if (outerBracketsOpen && !squareBracketsOpen)
                    {
                        yield return Result.Empty<Lexicons>(next.Location, "Unexpected left bracket");

                    }
                    else if(outerBracketsOpen && squareBracketsOpen)
                    {
                        yield return Result.Value(Lexicons.LEFT_CURLY_BRACKET, next.Location, next.Remainder);

                        innerBracketsOpen = true;
                    }

                    if(!outerBracketsOpen)
                        outerBracketsOpen = true;

                    next = next.Remainder.ConsumeChar();
                }
                else if (next.Value == '}')
                {
                    if (!outerBracketsOpen && squareBracketsOpen)
                        yield return Result.Empty<Lexicons>(next.Location, "Unexpected right bracket");
                    else if(outerBracketsOpen && squareBracketsOpen)
                    {
                        yield return Result.Value(Lexicons.RIGHT_CURLY_BRACKET, next.Location, next.Remainder);

                        innerBracketsOpen = false;
                    }

                    if(outerBracketsOpen && !squareBracketsOpen)
                        outerBracketsOpen = false;

                    next = next.Remainder.ConsumeChar();
                }
                else if (next.Value == '[')
                {
                    if (squareBracketsOpen)
                        yield return Result.Empty<Lexicons>(next.Location, "Unexpected left square bracket");
                    else
                        yield return Result.Value(Lexicons.LEFT_BRACKET, next.Location, next.Remainder);

                    squareBracketsOpen = true;

                    next = next.Remainder.ConsumeChar();
                }
                    
                else if (next.Value == ']')
                {
                    if (!squareBracketsOpen)
                        yield return Result.Empty<Lexicons>(next.Location, "Unexpected right square bracket");
                    else
                        yield return Result.Value(Lexicons.RIGHT_BRACKET, next.Location, next.Remainder);

                    squareBracketsOpen = false;

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
                else if (next.Value == '.')
                {
                    yield return Result.Value(Lexicons.DOT, next.Location, next.Remainder);
                    next = next.Remainder.ConsumeChar();
                }
                else
                {
                    if (char.IsDigit(next.Value))
                    {
                        var integer = Numerics.Integer(next.Location);
                        yield return Result.Value(Lexicons.NUMBER, next.Location, integer.Remainder);
                        next = integer.Remainder.ConsumeChar();
                    }
                    else if (next.Value == Convert.ToChar("'"))
                    {
                        textOpen = true;
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
                    else if (idOrKeyword == "nsum")
                        return Lexicons.NSUM;
                    else if (idOrKeyword == "navg")
                        return Lexicons.NAVG;
                    else if (idOrKeyword == "ncount")
                        return Lexicons.NCOUNT;
                    else if (idOrKeyword == "nmin")
                        return Lexicons.NMIN;
                    else if (idOrKeyword == "nmax")
                        return Lexicons.NMAX;
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
                  else if (idOrKeyword == "nsum")
                      return Lexicons.NSUM;
                  else if (idOrKeyword == "navg")
                      return Lexicons.NAVG;
                  else if (idOrKeyword == "ncount")
                      return Lexicons.NCOUNT;
                  else if (idOrKeyword == "nmin")
                      return Lexicons.NMIN;
                  else if (idOrKeyword == "nmax")
                      return Lexicons.NMAX;
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
            new AggregateOperator("nsum"),
            new AggregateOperator("navg"),
            new AggregateOperator("ncount"),
            new AggregateOperator("nmin"),
            new AggregateOperator("nmax"),
        }.ToImmutableDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);
    }
}
