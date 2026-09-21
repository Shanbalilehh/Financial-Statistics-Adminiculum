using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinancialStatisticsAdminiculum.Infrastructure.AI
{
    public class FunctionGemmaHttpClient : IFunctionGemmaClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FunctionGemmaHttpClient> _logger;
        private readonly string _endpoint;

        public FunctionGemmaHttpClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<FunctionGemmaHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var baseUrl = configuration["AI:FunctionGemmaUrl"] ?? "http://localhost:8080";
            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }
            _httpClient.BaseAddress = new Uri(baseUrl);
            _endpoint = configuration["AI:FunctionGemmaEndpoint"] ?? "predict";
        }

        public async Task<FunctionGemmaResponse> CallModelAsync(string prompt, object? toolSchemas = null, CancellationToken ct = default)
        {
            try
            {
                var payload = new
                {
                    prompt = prompt,
                    tools = toolSchemas
                };

                _logger.LogInformation("Invoking containerized FunctionGemma model at {BaseAddress}{Endpoint}", _httpClient.BaseAddress, _endpoint);

                using var response = await _httpClient.PostAsJsonAsync(_endpoint, payload, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogWarning("FunctionGemma container returned non-success status code {StatusCode}: {Error}", response.StatusCode, err);
                    return new FunctionGemmaResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Model service returned HTTP {(int)response.StatusCode}: {err}"
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<ModelInferenceResponse>(cancellationToken: ct);
                if (result == null)
                {
                    return new FunctionGemmaResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Received empty response from model container."
                    };
                }

                var toolCalls = new List<FunctionGemmaToolCall>();
                if (result.ToolCalls != null)
                {
                    foreach (var tc in result.ToolCalls)
                    {
                        toolCalls.Add(new FunctionGemmaToolCall
                        {
                            Id = tc.Id ?? Guid.NewGuid().ToString("N"),
                            Name = tc.Name ?? string.Empty,
                            Arguments = tc.Arguments ?? new Dictionary<string, object>()
                        });
                    }
                }

                return new FunctionGemmaResponse
                {
                    IsSuccess = true,
                    ToolCalls = toolCalls,
                    Content = result.Content
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to connect to containerized FunctionGemma service at {BaseAddress}", _httpClient.BaseAddress);
                return new FunctionGemmaResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Connection to AI model container failed: {ex.Message}"
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Timeout connecting to containerized FunctionGemma service.");
                return new FunctionGemmaResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "AI model inference timed out."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling containerized FunctionGemma service.");
                return new FunctionGemmaResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Inference error: {ex.Message}"
                };
            }
        }

        public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                using var response = await _httpClient.GetAsync("health", cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private class ModelInferenceResponse
        {
            public List<ModelToolCallDto>? ToolCalls { get; set; }
            public string? Content { get; set; }
        }

        private class ModelToolCallDto
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
            public Dictionary<string, object>? Arguments { get; set; }
        }
    }
}
