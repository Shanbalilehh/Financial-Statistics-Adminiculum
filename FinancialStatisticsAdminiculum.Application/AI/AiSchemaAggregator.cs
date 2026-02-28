using System.Reflection;

namespace FinancialStatisticsAdminiculum.Application.AI
{
    public static class AiSchemaAggregator
    {
        public static string BuildCombinedToolJson()
        {
            var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(IAiToolHandler)));

            var schemas = new List<string>();

            foreach (var type in handlerTypes)
            {
                // Look for the static method by name
                var schemaMethod = type.GetMethod("GetToolSchema", BindingFlags.Public | BindingFlags.Static);
                
                if (schemaMethod != null)
                {
                    string schema = (string)schemaMethod.Invoke(null, null)!;
                    schemas.Add(schema);
                }
            }

            return $"[{string.Join(",", schemas)}]";
        }
    }
}