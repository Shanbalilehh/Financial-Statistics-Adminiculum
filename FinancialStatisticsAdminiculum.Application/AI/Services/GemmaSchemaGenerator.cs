using System.Text;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;

namespace FinancialStatisticsAdminiculum.Application.AI.Services
{
    //Objetive: Generate Tools Schema from objects
    public static class GemmaSchemaGenerator
    {
        public static string GenerateDeclaration(IGemmaTool tool)
        {
            var sb = new StringBuilder();
            
            sb.Append($"<start_function_declaration>declaration:{tool.Name}{{");
            sb.Append($"description:<escape>{tool.Description}<escape>,");
            sb.Append("parameters:{properties:{");

            // Add parameters
            var properties = new List<string>();
            foreach (var param in tool.Parameters)
            {
                var propSb = new StringBuilder();
                propSb.Append($"{param.Key}:{{description:<escape>{param.Value.Description}<escape>,");
                
                if (param.Value.EnumValues != null && param.Value.EnumValues.Length > 0)
                {
                    var enums = string.Join(",", param.Value.EnumValues.Select(e => $"<escape>{e}<escape>"));
                    propSb.Append($"enum:[{enums}],");
                }
                
                propSb.Append($"type:<escape>{param.Value.Type}<escape>}}");
                properties.Add(propSb.ToString());
            }
            
            sb.Append(string.Join(",", properties));
            sb.Append("}"); // close properties

            // Add required fields
            var requiredKeys = tool.Parameters.Where(p => p.Value.IsRequired).Select(p => p.Key).ToList();
            if (requiredKeys.Any())
            {
                var requiredStr = string.Join(",", requiredKeys.Select(k => $"<escape>{k}<escape>"));
                sb.Append($",required:[{requiredStr}]");
            }

            sb.Append(",type:<escape>OBJECT<escape>} }<end_function_declaration>");
            
            return sb.ToString();
        }
    }
}