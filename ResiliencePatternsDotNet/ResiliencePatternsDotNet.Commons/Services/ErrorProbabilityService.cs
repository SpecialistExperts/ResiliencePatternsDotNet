﻿using System;

namespace ResiliencePatternsDotNet.Commons.Services
{
    public class ErrorProbabilityService
    {
        public static bool IsErrorRequest(int percent)
        {
            var r = new Random();
            var rInt = r.Next(0, 100);
            return rInt <= percent;
        }
    }
}