using FinancialStatisticsAdminiculum.Application.AI.Entities;
using FinancialStatisticsAdminiculum.Application.Interfaces;

namespace FinancialStatisticsAdminiculum.Application.AI.Services
{
    public class FunctionGemmaParser : IFunctionGemmaParser
    {
        public List<GemmaToolCall> ParseToolCalls(string modelOutput)
        {
            var results = new List<GemmaToolCall>();
            ReadOnlySpan<char> span = modelOutput.AsSpan();

            // FunctionGemma specific tokens
            ReadOnlySpan<char> startCallTag = "<start_function_call>call:";
            ReadOnlySpan<char> endCallTag = "<end_function_call>";

            while (!span.IsEmpty)
            {
                // 1. Find the start of a tool call
                int startIndex = span.IndexOf(startCallTag);
                if (startIndex == -1) break; // No more tool calls found

                // Move the span forward past the start tag
                span = span.Slice(startIndex + startCallTag.Length);

                // 2. Extract the function name (everything up to the opening brace '{')
                int braceIndex = span.IndexOf('{');
                if (braceIndex == -1) break; // Malformed output

                string functionName = span.Slice(0, braceIndex).ToString();
                span = span.Slice(braceIndex + 1);

                // 3. Isolate the arguments block
                int endBraceIndex = span.IndexOf('}');
                if (endBraceIndex == -1) break; // Malformed output

                ReadOnlySpan<char> argsSpan = span.Slice(0, endBraceIndex);
                
                // 4. Parse the arguments
                var arguments = new Dictionary<string, string>();
                ParseArguments(argsSpan, arguments);

                results.Add(new GemmaToolCall 
                { 
                    Name = functionName, 
                    Arguments = arguments 
                });

                // 5. Move past the closing tag to prepare for the next potential tool call
                int endIndex = span.IndexOf(endCallTag);
                if (endIndex != -1)
                {
                    span = span.Slice(endIndex + endCallTag.Length);
                }
                else
                {
                    break;
                }
            }

            return results;
        }

        private static void ParseArguments(ReadOnlySpan<char> argsSpan, Dictionary<string, string> arguments)
        {
            ReadOnlySpan<char> escapeTag = "<escape>";

            while (!argsSpan.IsEmpty)
            {
                // Find the key separator ':'
                int colonIndex = argsSpan.IndexOf(':');
                if (colonIndex == -1) break;

                // Extract the key
                string key = argsSpan.Slice(0, colonIndex).Trim().ToString();
                argsSpan = argsSpan.Slice(colonIndex + 1);

                ReadOnlySpan<char> valueSpan;

                // Check if the value is a string wrapped in <escape> tags
                if (argsSpan.StartsWith(escapeTag))
                {
                    argsSpan = argsSpan.Slice(escapeTag.Length);
                    int endEscapeIndex = argsSpan.IndexOf(escapeTag);
                    if (endEscapeIndex == -1) break; // Malformed escape sequence
                    
                    valueSpan = argsSpan.Slice(0, endEscapeIndex);
                    argsSpan = argsSpan.Slice(endEscapeIndex + escapeTag.Length);
                    
                    // Skip the trailing comma if there are more arguments
                    if (!argsSpan.IsEmpty && argsSpan[0] == ',')
                    {
                        argsSpan = argsSpan.Slice(1);
                    }
                }
                else
                {
                    // Unescaped value (e.g., numbers like temperature:15)
                    int commaIndex = argsSpan.IndexOf(',');
                    if (commaIndex == -1)
                    {
                        valueSpan = argsSpan; // Last argument
                        argsSpan = ReadOnlySpan<char>.Empty;
                    }
                    else
                    {
                        valueSpan = argsSpan.Slice(0, commaIndex);
                        argsSpan = argsSpan.Slice(commaIndex + 1);
                    }
                }

                arguments[key] = valueSpan.Trim().ToString();
            }
        }
        

    }
}
