using System.Text;
using FinancialStatisticsAdminiculum.Infrastructure.AI.Entities;
using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Infrastructure.AI.Services
{
    public class GemmaPromptFormatter
    {
        public static string BuildPrompt(List<ChatMessage> history)
        {
            var sb = new StringBuilder();
            sb.Append("<bos>"); // Beginning of sequence

            for (int i = 0; i < history.Count; i++)
            {
                var message = history[i];

                switch (message.Role)
                {
                    case ChatRole.Developer:
                        sb.Append("<start_of_turn>developer\n");
                        sb.Append(message.Content);
                        sb.Append("<end_of_turn>\n");
                        break;

                    case ChatRole.User:
                        sb.Append("<start_of_turn>user\n");
                        sb.Append(message.Content);
                        sb.Append("<end_of_turn>\n");
                        break;

                    case ChatRole.Model:
                        sb.Append("<start_of_turn>model\n");
                        
                        if (message.ToolCalls != null && message.ToolCalls.Count > 0)
                        {
                            // The model generated a tool call. 
                            // Note: FunctionGemma does NOT output <end_of_turn> after a tool call.
                            // It waits for the tool response first.
                            sb.Append(message.Content); 
                        }
                        else
                        {
                            // Standard text response
                            sb.Append(message.Content);
                            sb.Append("<end_of_turn>\n");
                        }
                        break;

                    case ChatRole.Tool:
                        // Append the execution result directly into the model's ongoing turn
                        sb.Append(message.Content);
                        
                        // If this is the last message, we need to add the generation prompt 
                        // so the model knows it's time to read the tool output and reply to the user.
                        if (i == history.Count - 1)
                        {
                            sb.Append("<start_of_turn>model\n");
                        }
                        break;
                }
            }

            // Add the generation prompt if the last message was from the user
            if (history[^1].Role == ChatRole.User)
            {
                sb.Append("<start_of_turn>model\n");
            }

            return sb.ToString();
        }
    }
}