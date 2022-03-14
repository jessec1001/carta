using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CartaCore.Documentation;
using CartaCore.Extensions.Documentation;
using CartaCore.Operations.Attributes;
using CsvHelper;
using NJsonSchema;

namespace CartaCore.Operations.Arithmetic
{
    /// <summary>
    /// The input for the <see cref="ArithmeticExpressionOperation" /> operation.
    /// </summary>
    public struct ArithmeticExpressionOperationIn
    {
        /// <summary>
        /// The mathematical expression that represents an operation on some variables.
        /// </summary>
        [FieldRequired]
        [FieldName("Expression")]
        public string Expression { get; set; }

        /// <summary>
        /// The value that should be substituted for the corresponding variable symbol in the mathematical expression.
        /// </summary>
        public Dictionary<string, double> Values { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ArithmeticExpressionOperation" /> operation.
    /// </summary>
    public struct ArithmeticExpressionOperationOut
    {
        /// <summary>
        /// The resulting value of the arithmetic expression.
        /// </summary>
        [FieldName("Result")]
        public double Result { get; set; }
    }

    // TODO: We can do an interpreter optimization by compiling the expression only once if it is defined in the
    //       default values and not in the input values.

    /// <summary>
    /// An operation that computes the result of an arithmetic expression by substituting values for variables that were
    /// specified in the expression.
    /// The following operations are supported:
    /// - Arithmetic (Add: `+`, Subtract: `-`, Multiply: `*`, Divide: `/`, Power: `^`, Remainder: `%`)
    /// - Comparison (Equal: `=`, Not Equal: `~=`, Less Than: `&lt;`, Less Than or Equal: `&lt;=`, Greater Than: `&gt;`, Greater Than or Equal: `&gt;=`)
    /// - Logical (And: `&amp;`, Or: `|`, Not: `~`)
    /// - Grouping: `()`
    /// </summary>
    [OperationTag(OperationTags.Arithmetic)]
    [OperationName(Display = "Arithmetic Expression", Type = "arithmeticExpression")]
    public class ArithmeticExpressionOperation : TypedOperation
    <
        ArithmeticExpressionOperationIn,
        ArithmeticExpressionOperationOut
    >
    {
        /// <summary>
        /// The interpreter for the arithmetic expression.
        /// </summary>
        private class ArithmeticInterpreter
        {
            /// <summary>
            /// Represents the type of function that performs a valuation.
            /// </summary>
            private delegate double Valuation(Dictionary<string, double> values);

            /// <summary>
            /// The arithmetic expression to interpret.
            /// </summary>
            private string Expression { get; init; }
            /// <summary>
            /// The function used to evaluate the expression.
            /// </summary>
            private Valuation Evaluation { get; set; }

            /// <summary>
            /// A number is anything of the form '123' or '.123' or '1.23' but not of '1.'. Negations are handled
            /// separately. This regular expression pattern matches numbers.
            /// </summary>
            private static readonly Regex NumberPattern = new(@"(\d+|\d*\.\d+)", RegexOptions.Compiled);
            /// <summary>
            /// A symbol is any consecutive sequence of characters that may optionally end in a sequence of digits. This
            /// regular expression pattern matches symbols.
            /// </summary>
            private static readonly Regex SymbolPattern = new(@"[a-zA-Z]+\d*", RegexOptions.Compiled);
            /// <summary>
            /// This regular expression pattern matches whitespace.
            /// </summary>
            private static readonly Regex SpacePattern = new(@"\s+", RegexOptions.Compiled);

            /// <summary>
            /// These are the operational token strings that are used in lexing.
            /// </summary>
            private static readonly string[] OperationTokens = new string[]
            {
                "(", ")",                           // Grouping
                "<=", ">=", "<", ">", "~=", "=",    // Comparison
                "&", "|", "~",                      // Logic
                "+", "-", "*", "/", "^", "%"        // Arithmetic
            };

            /// <summary>
            /// Constructs a new instance of the <see cref="ArithmeticInterpreter" /> on a specified arithmetic
            /// expression. 
            /// </summary>
            /// <param name="expression">The arithmetic expression.</param>
            public ArithmeticInterpreter(string expression)
            {
                if (expression is null)
                    throw new ArgumentNullException(nameof(expression));

                Expression = expression;
                Evaluation = ParseExpression(LexExpression(expression));
            }

            /// <summary>
            /// Lexes the specified expression into a list of tokens.
            /// </summary>
            /// <returns>A list of strings representing tokens.</returns>
            private static List<string> LexExpression(string expression)
            {
                // We store the tokens in this list.
                List<string> tokens = new();

                // We try to parse every part of the expression.
                string remaining = expression;
                while (remaining.Length > 0)
                {
                    // This string represents the matching text of the remaining expression. If null by the end of this
                    // matching expression block, this represents that nothing was matched.
                    string match = null;

                    // We check if any of the operational tokens is the next token.
                    foreach (string operation in OperationTokens)
                    {
                        if (remaining.StartsWith(operation))
                        {
                            match = operation;
                            break;
                        }
                    }

                    // If we did not match one of the operational tokens, we check if the remaining text matches one of
                    // the more complex token types.
                    if (match is null)
                    {
                        Match spaceMatch = SpacePattern.Match(remaining);
                        Match numberMatch = NumberPattern.Match(remaining);
                        Match symbolMatch = SymbolPattern.Match(remaining);
                        if (spaceMatch.Success && spaceMatch.Index == 0)
                        {
                            // We ignore whitespace.
                            match = spaceMatch.Value;
                            remaining = remaining[match.Length..];
                            continue;
                        }
                        else if (numberMatch.Success && numberMatch.Index == 0)
                            match = numberMatch.Value;
                        else if (symbolMatch.Success && symbolMatch.Index == 0)
                            match = symbolMatch.Value;
                    }

                    // If there is a syntax error, we raise an argument exception.
                    if (match is null)
                        throw new ArgumentException($"Unknown symbol in arithmetic expression (${remaining[0]}).");
                    else
                    {
                        tokens.Add(match);
                        remaining = remaining[match.Length..];
                    }
                }
                return tokens;
            }
            /// <summary>
            /// Compiles the specified expression into an expression tree.
            /// </summary>
            private static Valuation ParseExpression(List<string> tokens)
            {
                // This list of values will maintain the current state of the parser as a list of undigested tokens and
                // executable expressions.
                List<object> values = new(tokens);

                // We perform a first pass to convert numeric and symbolic tokens into expressions.
                for (int k = 0; k < values.Count; k++)
                {
                    // These values represent the undigested tokens that we perform rules on. 
                    if (values[k] is string token)
                    {
                        // Check if number.
                        if (double.TryParse(token, out double number))
                            values[k] = (Valuation)((valuation) => number);

                        // Check if symbol.
                        else if (!OperationTokens.Contains(token))
                            values[k] = (Valuation)((valuation) => valuation.GetValueOrDefault(token));
                    }
                }

                // We repeat parsing values until nothing has matched anymore.
                while (true)
                {
                    // We use this matched value to determine whether we are done parsing yet.
                    // Notice, we must handle right-to-left and left-to-right associativity differently.
                    bool matched = false;
                    for (int k = values.Count - 1; k >= 0; k--)
                    {
                        // Match binary operations.
                        if (k + 3 <= values.Count)
                        {
                            if (
                                values[k + 0] is Valuation valuation1 &&
                                values[k + 1] is string token &&
                                values[k + 2] is Valuation valuation2
                            )
                            {
                                Valuation result = null;
                                switch (token)
                                {
                                    case "^":
                                        result = (values) => Math.Pow(valuation1(values), valuation2(values));
                                        break;
                                }
                                if (result is not null)
                                {
                                    values.RemoveRange(k, 3);
                                    values.Insert(k, result);
                                    matched = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (matched) continue;
                    for (int k = 0; k < values.Count; k++)
                    {
                        // Match binary operations.
                        if (k + 3 <= values.Count)
                        {
                            if (
                                values[k + 0] is Valuation valuation1 &&
                                values[k + 1] is string token &&
                                values[k + 2] is Valuation valuation2
                            )
                            {
                                Valuation result = null;
                                switch (token)
                                {
                                    // Arithmetic
                                    case "*":
                                        result = (values) => valuation1(values) * valuation2(values);
                                        break;
                                    case "/":
                                        result = (values) => valuation1(values) / valuation2(values);
                                        break;
                                    case "%":
                                        result = (values) => valuation1(values) % valuation2(values);
                                        break;
                                    case "+":
                                        result = (values) => valuation1(values) + valuation2(values);
                                        break;
                                    case "-":
                                        result = (values) => valuation1(values) - valuation2(values);
                                        break;

                                    // Comparison
                                    case "<":
                                        result = (values) => valuation1(values) < valuation2(values) ? 1 : 0;
                                        break;
                                    case "<=":
                                        result = (values) => valuation1(values) <= valuation2(values) ? 1 : 0;
                                        break;
                                    case ">":
                                        result = (values) => valuation1(values) > valuation2(values) ? 1 : 0;
                                        break;
                                    case ">=":
                                        result = (values) => valuation1(values) >= valuation2(values) ? 1 : 0;
                                        break;
                                    case "=":
                                        result = (values) => valuation1(values) == valuation2(values) ? 1 : 0;
                                        break;
                                    case "~=":
                                        result = (values) => valuation1(values) != valuation2(values) ? 1 : 0;
                                        break;

                                    // Logic
                                    case "|":
                                        result = (values) => valuation1(values) != 0 || valuation2(values) != 0 ? 1 : 0;
                                        break;
                                    case "&":
                                        result = (values) => valuation1(values) != 0 && valuation2(values) != 0 ? 1 : 0;
                                        break;
                                }
                                if (result is not null)
                                {
                                    values.RemoveRange(k, 3);
                                    values.Insert(k, result);
                                    matched = true;
                                    break;
                                }
                            }
                        }

                        // Match unary operations.
                        if (matched) continue;
                        if (k + 2 <= values.Count)
                        {
                            if (
                                values[k + 0] is string token &&
                                values[k + 1] is Valuation valuation
                            )
                            {
                                Valuation result = null;
                                switch (token)
                                {
                                    case "-":
                                        result = (values) => -valuation(values);
                                        break;
                                    case "~":
                                        result = (values) => valuation(values) == 0 ? 1 : 0;
                                        break;
                                }
                                if (result is not null)
                                {
                                    values.RemoveRange(k, 2);
                                    values.Insert(k, result);
                                    matched = true;
                                    break;
                                }
                            }
                        }

                        // Match grouping.
                        if (matched) continue;
                        if (k + 3 <= values.Count)
                        {
                            if (
                                values[k + 0] is "(" &&
                                values[k + 1] is Valuation valuation &&
                                values[k + 2] is ")"
                            )
                            {
                                values.RemoveAt(k + 2);
                                values.RemoveAt(k + 0);
                                matched = true;
                                break;
                            }
                        }
                    }

                    if (!matched)
                        break;
                }

                // With no tokens, we simply return zero.
                // If there is a syntax error (too many values), we raise an argument exception.
                // Otherwise, we assume that the remaining value is the evaluator function.
                if (values.Count == 0) return (_) => 0;
                if (values.Count > 1) throw new ArgumentException("Invalid arithmetic expression.");
                return (Valuation)values.First();
            }

            /// <summary>
            /// Finds the set of symbols that the expression contains. Values for these symbols should be specified when
            /// the expression is interpreted.
            /// </summary>
            public string[] FindSymbols()
            {
                return SymbolPattern
                    .Matches(Expression)
                    .Select(match => match.Value)
                    .ToArray();
            }
            /// <summary>
            /// Interprets the arithmetic expression with a valuation of variables.
            /// </summary>
            /// <param name="values">The valuations of the variables.</param>
            /// <returns>The result of the arithmetic expression.</returns>
            public double Interpret(Dictionary<string, double> values)
            {
                // We have constructed an abstract syntax tree that is can be evaluated on the values.
                return Evaluation(values);
            }
        }

        /// <inheritdoc />
        public override Task<ArithmeticExpressionOperationOut> Perform(ArithmeticExpressionOperationIn input)
        {
            // We simply use the interpreter to do all of the work for us.
            ArithmeticInterpreter interpreter = new(input.Expression);
            return Task.FromResult(
                new ArithmeticExpressionOperationOut() { Result = interpreter.Interpret(input.Values) }
            );
        }

        /// <inheritdoc />
        public override async Task<ArithmeticExpressionOperationIn> ConvertInput(OperationJob job, IDictionary<string, object> inputs)
        {
            // This override for converting input values guarantees that the symbols in the expression are assigned to.
            // Get the base inputs.
            ArithmeticExpressionOperationIn input = await base.ConvertInput(job, inputs);
            input.Values = new Dictionary<string, double>();

            // Try to find the symbols in the expression.
            string[] symbols = Array.Empty<string>();
            try
            {
                ArithmeticInterpreter interpreter = new(input.Expression);
                symbols = interpreter.FindSymbols();
            }
            catch (ArgumentException) { }

            // We get each of the symbols and add them to the input appropriately.
            await foreach (OperationFieldDescriptor field in GetInputFields(job))
            {
                foreach (string symbol in symbols)
                {
                    if (field.Name == symbol)
                    {
                        if (inputs.TryGetValue(field.Name, out object value))
                            input.Values.Add(symbol, (double)await ConvertInputField(field, value, job));
                        else
                            input.Values.Add(symbol, default);
                        break;
                    }
                }
            }
            return input;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<OperationFieldDescriptor> GetInputFields(OperationJob job)
        {
            // Hide the values dictionary from the inputs.
            string expression = null;
            IDictionary<string, object> inputs = MergeDefaults(job.Input, Defaults);
            await foreach (OperationFieldDescriptor inputDescriptor in base.GetInputFields(job))
            {
                if (inputDescriptor.Name == nameof(ArithmeticExpressionOperationIn.Expression))
                {
                    if (inputs.TryGetValue(inputDescriptor.Name, out object value))
                        expression = (string)await ConvertInputField(inputDescriptor, value, job);
                }
                if (inputDescriptor.Name != nameof(ArithmeticExpressionOperationIn.Values))
                    yield return inputDescriptor;
            }

            // If the expression has not been specified, we should not add any fields.
            if (expression is null) yield break;

            // Fetch the documentation for the value field.
            StandardDocumentation documentation = typeof(ArithmeticExpressionOperationIn)
                .GetProperty(nameof(ArithmeticExpressionOperationIn.Values))
                .GetDocumentation<StandardDocumentation>();

            // If the expression is invalid, we should not add any fields.
            string[] symbols = Array.Empty<string>();
            try
            {
                ArithmeticInterpreter interpreter = new(expression);
                symbols = interpreter.FindSymbols();
            }
            catch (ArgumentException) { }

            // Yield the value inputs based on the interpreted expression.
            foreach (string symbol in symbols)
            {
                // Generate the schema for the value field.
                JsonSchema schema = OperationHelper.GenerateSchema(typeof(double));
                schema.Title = symbol;
                schema.Description = documentation.UncontractedSummary;

                yield return new OperationFieldDescriptor()
                {
                    Name = symbol,
                    Type = typeof(double),
                    Schema = schema,
                    Attributes = new List<Attribute>()
                };
            }

        }
    }
}