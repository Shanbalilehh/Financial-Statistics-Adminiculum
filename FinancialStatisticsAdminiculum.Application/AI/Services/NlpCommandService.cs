using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Tools;
using FinancialStatisticsAdminiculum.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace FinancialStatisticsAdminiculum.Application.AI.Services
{
    public interface INlpCommandService
    {
        Task<NlpCommandResultDto> ProcessCommandAsync(Guid workspaceId, string prompt, CancellationToken ct = default);
    }

    public class NlpCommandResultDto
    {
        public Guid CommandId { get; set; }
        public string Prompt { get; set; } = null!;
        public string Status { get; set; } = "Executed";
        public string? ResolvedTool { get; set; }
        public Dictionary<string, object>? ExtractedArguments { get; set; }
        public List<CanvasMutationDto> Mutations { get; set; } = new();
        public string? Explanation { get; set; }
    }

    public class CanvasMutationDto
    {
        public string Action { get; set; } = null!;
        public object Payload { get; set; } = null!;
    }

    public class NlpCommandService : INlpCommandService
    {
        private readonly ILogger<NlpCommandService> _logger;

        public NlpCommandService(ILogger<NlpCommandService> logger)
        {
            _logger = logger;
        }

        public Task<NlpCommandResultDto> ProcessCommandAsync(Guid workspaceId, string prompt, CancellationToken ct = default)
        {
            _logger.LogInformation("Processing NLP command for workspace {WorkspaceId}: {Prompt}", workspaceId, prompt);

            var commandId = Guid.NewGuid();
            var mutations = new List<CanvasMutationDto>();
            var extractedArgs = new Dictionary<string, object>();
            string resolvedTool = "generic_action";
            string explanation = "Interpreted and applied canvas mutations.";

            var lower = prompt.ToLowerInvariant();

            // Detect Symbol (e.g. AAPL, MSFT, SPY, QQQ)
            string symbol = "AAPL";
            var symbolMatch = Regex.Match(prompt, @"\b(AAPL|MSFT|SPY|QQQ|BTC-USD|NVDA|TSLA)\b", RegexOptions.IgnoreCase);
            if (symbolMatch.Success)
            {
                symbol = symbolMatch.Value.ToUpperInvariant();
                extractedArgs["symbol"] = symbol;
            }

            var priceStreamId = Guid.NewGuid();
            var priceStreamPayload = new
            {
                id = priceStreamId,
                type = "PriceStream",
                label = $"Asset: {symbol}",
                position = new { x = 120, y = 200 },
                parameters = new Dictionary<string, object> { { "symbol", symbol }, { "lookback", 100 } }
            };

            // Detect Volatility Request
            if (lower.Contains("volatil") || lower.Contains("vol"))
            {
                resolvedTool = VolatilityToolHandler.ToolName;
                int period = 30;
                var periodMatch = Regex.Match(lower, @"(\d+)[ -]?(day|period|d)?\s+volatility");
                if (periodMatch.Success && int.TryParse(periodMatch.Groups[1].Value, out var p))
                {
                    period = p;
                }
                extractedArgs["period"] = period;

                var volEntityId = Guid.NewGuid();
                var volPayload = new
                {
                    id = volEntityId,
                    type = "VolatilityEstimator",
                    label = $"Volatility ({period}d)",
                    position = new { x = 440, y = 200 },
                    parameters = new Dictionary<string, object> { { "period", period }, { "annualizationFactor", 252 } }
                };

                mutations.Add(new CanvasMutationDto { Action = "ADD_ENTITY", Payload = priceStreamPayload });
                mutations.Add(new CanvasMutationDto { Action = "ADD_ENTITY", Payload = volPayload });
                mutations.Add(new CanvasMutationDto
                {
                    Action = "ADD_CONNECTION",
                    Payload = new
                    {
                        id = $"conn_{priceStreamId}->{volEntityId}",
                        sourceEntityId = priceStreamId,
                        sourcePortId = "out_series",
                        targetEntityId = volEntityId,
                        targetPortId = "in_series"
                    }
                });

                // Detect nested trigger
                if (lower.Contains("trigger") || lower.Contains("alert") || lower.Contains("exceed"))
                {
                    double threshold = 25.0;
                    var threshMatch = Regex.Match(lower, @"(exceeds?|>|above|over)\s+(\d+(\.\d+)?)%?");
                    if (threshMatch.Success && double.TryParse(threshMatch.Groups[2].Value, out var t))
                    {
                        threshold = t;
                    }
                    extractedArgs["threshold"] = threshold;

                    var triggerId = Guid.NewGuid();
                    var triggerPayload = new
                    {
                        id = triggerId,
                        type = "SignalTrigger",
                        label = "Signal Trigger",
                        position = new { x = 760, y = 200 },
                        parameters = new Dictionary<string, object> { { "threshold", threshold }, { "condition", "GreaterThan" } }
                    };

                    mutations.Add(new CanvasMutationDto { Action = "ADD_ENTITY", Payload = triggerPayload });
                    mutations.Add(new CanvasMutationDto
                    {
                        Action = "ADD_CONNECTION",
                        Payload = new
                        {
                            id = $"conn_{volEntityId}->{triggerId}",
                            sourceEntityId = volEntityId,
                            sourcePortId = "out_series",
                            targetEntityId = triggerId,
                            targetPortId = "in_series"
                        }
                    });
                    explanation = $"Synthesized a {period}-day volatility pipeline for {symbol} wired to an alert trigger at > {threshold}%.";
                }
                else
                {
                    explanation = $"Added a {period}-day volatility pipeline for asset {symbol}.";
                }
            }
            // Detect Moving Average Request
            else if (lower.Contains("moving average") || lower.Contains("sma") || lower.Contains("ema"))
            {
                resolvedTool = SmaToolHandler.ToolName;
                int period = 20;
                var pMatch = Regex.Match(lower, @"(\d+)[ -]?(day|period|d)?\s+(moving average|sma|ema)");
                if (pMatch.Success && int.TryParse(pMatch.Groups[1].Value, out var p))
                {
                    period = p;
                }
                string method = lower.Contains("ema") ? "EMA" : "SMA";
                extractedArgs["period"] = period;
                extractedArgs["method"] = method;

                var maId = Guid.NewGuid();
                var maPayload = new
                {
                    id = maId,
                    type = "MovingAverage",
                    label = $"{method} ({period})",
                    position = new { x = 440, y = 200 },
                    parameters = new Dictionary<string, object> { { "period", period }, { "method", method } }
                };

                mutations.Add(new CanvasMutationDto { Action = "ADD_ENTITY", Payload = priceStreamPayload });
                mutations.Add(new CanvasMutationDto { Action = "ADD_ENTITY", Payload = maPayload });
                mutations.Add(new CanvasMutationDto
                {
                    Action = "ADD_CONNECTION",
                    Payload = new
                    {
                        id = $"conn_{priceStreamId}->{maId}",
                        sourceEntityId = priceStreamId,
                        sourcePortId = "out_series",
                        targetEntityId = maId,
                        targetPortId = "in_series"
                    }
                });

                explanation = $"Created {method} ({period} periods) for {symbol}.";
            }
            else
            {
                // Generic Entity Creation fallback
                mutations.Add(new CanvasMutationDto { Action = "ADD_ENTITY", Payload = priceStreamPayload });
                explanation = $"Added {symbol} price stream entity to canvas.";
            }

            return Task.FromResult(new NlpCommandResultDto
            {
                CommandId = commandId,
                Prompt = prompt,
                Status = "Executed",
                ResolvedTool = resolvedTool,
                ExtractedArguments = extractedArgs,
                Mutations = mutations,
                Explanation = explanation
            });
        }
    }
}
