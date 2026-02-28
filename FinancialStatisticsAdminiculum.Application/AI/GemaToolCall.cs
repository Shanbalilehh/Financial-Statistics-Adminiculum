using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinancialStatisticsAdminiculum.Application.AI
{
    public class GemaToolCall
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // JsonElement holds the raw JSON tree, allowing infinite flexibility
        [JsonPropertyName("arguments")]
        public JsonElement Arguments { get; set; } 
    }
}