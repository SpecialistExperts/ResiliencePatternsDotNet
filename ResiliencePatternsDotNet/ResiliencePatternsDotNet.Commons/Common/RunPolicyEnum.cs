﻿namespace ResiliencePatternsDotNet.Commons.Common
{
    public enum RunPolicyEnum
    {
        RETRY = 0,
        CIRCUIT_BREAKER = 1,
        ALL = 2,
        NONE = 3
    }
}