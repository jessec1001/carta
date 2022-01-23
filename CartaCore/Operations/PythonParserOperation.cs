using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;

using CliWrap;

namespace CartaCore.Operations.Parser
{

    /// <summary>
    /// The input for the <see cref="PythonParserOperation" /> operation.
    /// </summary>
    public struct InputPythonParserOperation
    {
        /// <summary>
        /// A stream containing the input to parse.
        /// </summary>
        public Stream Stream;

        /// <summary>
        /// A string containing the name of the Python script to execute.
        /// </summary>
        public string PythonScript;

        /// <summary>
        /// A list of strings to pass as arguments to the Python script.
        /// </summary>
        public List<string> Arguments;
    }

    /// <summary>
    /// The output for the <see cref="PythonParserOperation" /> operation.
    /// </summary>
    public struct OutputPythonParserOperation
    {
        /// <summary>
        /// A stream containing the parsed output
        /// </summary>
        public Stream Stream;
    }

    /// <summary>
    /// Operation that calls a python script that expects a stream with contents to parse from standard input,
    /// and that writes the parsed output to standard output.
    /// </summary>
    public class PythonParserOperation
    {

        private static string DataBaseDirectory = @"data";
        private static string PythonBaseDirectory = @"python";
        private static string ParserDirectory = @"parsers";

        /// <summary>
        /// Perform the operation
        /// </summary>
        /// <param name="input">An <see cref="InputPythonParserOperation" /> instance.</param>
        /// <returns>An <see cref="OutputPythonParserOperation" /> instance.</returns>
        public async Task<OutputPythonParserOperation> Perform(
            InputPythonParserOperation input)
        {
            // Initialize variables
            string pythonPath = Path.Combine(PythonBaseDirectory, ParserDirectory, input.PythonScript);
            string outputFile = Path.Combine(DataBaseDirectory, ParserDirectory, $"{Guid.NewGuid()}.txt");
            List<string> arguments;
            if (input.Arguments is null)
                arguments = new() { pythonPath };
            else
            {
                arguments = input.Arguments;
                arguments.Insert(0, pythonPath);
            }

            // Run command
            Console.WriteLine($"Running command 'python {String.Join(" ", arguments)}' and sending output to " +
                $"file '{outputFile}'");
            await Cli.Wrap("python")
                .WithArguments(arguments)
                .WithStandardInputPipe(PipeSource.FromStream(input.Stream))
                .WithStandardOutputPipe(PipeTarget.ToFile(outputFile))
                .ExecuteAsync();

            // Return output
            return new OutputPythonParserOperation { Stream = File.OpenRead(outputFile) };
        }

    }

}