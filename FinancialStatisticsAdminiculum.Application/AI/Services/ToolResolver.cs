using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialStatisticsAdminiculum.Application.AI.Services
{
    public class ToolResolver : IToolResolver
    {
        private readonly IServiceProvider _provider;

        public ToolResolver(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IGemmaTool? Resolve(string name) => _provider.GetKeyedService<IGemmaTool>(name);
    }
}