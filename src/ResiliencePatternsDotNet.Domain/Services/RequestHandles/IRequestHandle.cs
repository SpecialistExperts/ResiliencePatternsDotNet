﻿using System.Net.Http;
using ResiliencePatternsDotNet.Domain.Common;
using ResiliencePatternsDotNet.Domain.Configurations;

namespace ResiliencePatternsDotNet.Domain.Services.RequestHandles
{
    public interface IRequestHandle
    {
        HttpResponseMessage HandleRequest(int probabilityErrorPercent, UrlFetchConfigurationSection success, UrlFetchConfigurationSection error);
    }
}