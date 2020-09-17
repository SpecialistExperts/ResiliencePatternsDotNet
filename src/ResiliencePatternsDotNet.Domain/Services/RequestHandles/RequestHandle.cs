using System;
using System.Net.Http;
using ResiliencePatternsDotNet.Domain.Common;
using ResiliencePatternsDotNet.Domain.Configurations;
using ResiliencePatternsDotNet.Domain.Entities.Enums;
using ResiliencePatternsDotNet.Domain.Exceptions;
using ResiliencePatternsDotNet.Domain.Services.Resiliences;
using Toxiproxy.Net;
using Toxiproxy.Net.Toxics;

namespace ResiliencePatternsDotNet.Domain.Services.RequestHandles
{
    public class RequestHandle : IRequestHandle
    {
        private readonly IResiliencePatterns _resiliencePatterns;
        private readonly ConfigurationSection _configurationSection;
        private readonly MetricService _metrics;

        public RequestHandle(IResiliencePatterns resiliencePatterns, ConfigurationSection configurationSection,
            MetricService metrics)
        {
            _resiliencePatterns = resiliencePatterns;
            _configurationSection = configurationSection;
            _metrics = metrics;
            CreateCustomMetric();
        }

        private void CreateCustomMetric()
        {
            switch (_configurationSection.RunPolicy)
            {
                case RunPolicyEnum.RETRY:
                    _metrics.CreateRetryCustom();
                    break;
                case RunPolicyEnum.CIRCUIT_BREAKER:
                    _metrics.CreateCircuitBrekerCustom();
                    break;
                case RunPolicyEnum.ALL:
                    break;
                case RunPolicyEnum.NONE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public HttpResponseMessage HandleRequest(int probabilityErrorPercent, UrlFetchConfigurationSection success, UrlFetchConfigurationSection error)
        {
            return ConfigureProxy(probabilityErrorPercent, success, error, () =>
            {
                return _configurationSection.RunPolicy switch
                {
                    RunPolicyEnum.RETRY => HandleClientResult(_resiliencePatterns.RetryPolicy
                        .ExecuteAndCapture(() => MakeRequest(probabilityErrorPercent, success, error))
                        .Result),
                    RunPolicyEnum.CIRCUIT_BREAKER => HandleClientResult(_resiliencePatterns.CircuitBreakerPolicy
                        .ExecuteAndCapture(() => MakeRequest(probabilityErrorPercent, success, error))
                        .Result),
                    RunPolicyEnum.ALL => HandleClientResult(_resiliencePatterns.RetryPolicy.ExecuteAndCapture(() =>
                            _resiliencePatterns.CircuitBreakerPolicy.Execute(() =>
                                MakeRequest(probabilityErrorPercent, success, error)))
                        .Result),
                    RunPolicyEnum.NONE => HandleClientResult(MakeRequest(probabilityErrorPercent, success, error)),
                    _ => throw new ArgumentOutOfRangeException()
                };
            });
            
        }

        private HttpResponseMessage ConfigureProxy(in int probabilityErrorPercent, UrlFetchConfigurationSection success,
            UrlFetchConfigurationSection error, Func<HttpResponseMessage> runPolicy)
        {
            using (var connection = new Connection(true))
            {
                var client = connection.Client();

                //proxy all traffic from 127.0.0.1:44399 to 127.0.0.1:5000
                var localToGoogleProxy = new Proxy() { 
                    Name = "localToGoogle", 
                    Enabled = true, 
                    Listen = "127.0.0.1:44399", 
                    Upstream = _configurationSection.UrlConfiguration.BaseUrl 
                };
                            
                client.Add(localToGoogleProxy);

                        
                var proxy = client.FindProxy("localToGoogle");

                var timeoutProxy = new TimeoutToxic();
                timeoutProxy.Attributes.Timeout = 100;
                timeoutProxy.Toxicity = 1.0;

                proxy.Add(timeoutProxy);
                proxy.Update();

                return runPolicy();
            }
        }

        private HttpResponseMessage MakeRequest(int probabilityErrorPercent, UrlFetchConfigurationSection success, UrlFetchConfigurationSection error)
        {
            string actionMethod;
            string actionUrl;
            
            if (ErrorProbabilityService.IsDalayRequest(probabilityErrorPercent))
            {
                _metrics.IncrementeResilienceModuleError();
                actionMethod = error.Method;
                actionUrl = error.Url;
                Console.WriteLine($"Request With Delay!");
            }
            else
            {
                _metrics.IncrementeResilienceModuleSuccess();
                actionMethod = success.Method;
                actionUrl = success.Url;
                Console.WriteLine($"Request Normal!");
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    
                        
                        
                        httpClient.BaseAddress = new Uri("http://localhost:44399");
                        httpClient.Timeout =
                            TimeSpan.FromMilliseconds(_configurationSection.RequestConfiguration.Timeout);
                        var methodEnum = new HttpMethod(actionMethod);

                        var result = httpClient.SendAsync(new HttpRequestMessage(methodEnum, actionUrl)).GetAwaiter().GetResult();
                        Console.WriteLine($"Result: {result.StatusCode}");

                        if (_configurationSection.RunPolicy != RunPolicyEnum.NONE && !result.IsSuccessStatusCode)
                            throw new RequestException(result);

                        return result;
                    }
                    

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private HttpResponseMessage HandleClientResult(HttpResponseMessage result)
        {
            if (result?.IsSuccessStatusCode ?? false)
                _metrics.IncrementClientSuccess();
            else
                _metrics.IncrementClientError();
            
            return result;
        }
    }
}