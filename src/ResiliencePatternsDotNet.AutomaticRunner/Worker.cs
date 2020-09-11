using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResiliencePatternsDotNet.AutomaticRunner.Services;

namespace ResiliencePatternsDotNet.AutomaticRunner
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ScenarioService _scenarioService;

        public Worker(ILogger<Worker> logger, ScenarioService scenarioService)
        {
            _logger = logger;
            _scenarioService = scenarioService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            _scenarioService.ProcessScenarios();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}