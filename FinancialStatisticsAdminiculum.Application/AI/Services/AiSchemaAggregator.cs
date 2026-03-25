using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using System.Text;

namespace FinancialStatisticsAdminiculum.Application.AI.Services
{
    //Objective: Tools Schemas ready for passing to model (developer)
    public class AiSchemaAggregator : IAiSchemaAggregator
    {
        private readonly IEnumerable<IGemmaTool> _availableTools;

        public AiSchemaAggregator(IEnumerable<IGemmaTool> availableTools)
        {
            _availableTools = availableTools;
        }

        public string BuildCombinedTool()
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a model that can do function calling with the following functions");
            
            foreach (var tool in _availableTools)
            {
                // Using the custom formatter we built in the previous step!
                sb.Append(GemmaSchemaGenerator.GenerateDeclaration(tool));
            }
            
            return sb.ToString();
        }
    }
}