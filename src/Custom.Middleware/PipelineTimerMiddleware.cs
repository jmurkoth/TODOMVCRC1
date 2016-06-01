﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Custom.Middleware
{
    public class PipelineTimerMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger<PipelineTimerMiddleware> _logger;

        public PipelineTimerMiddleware(RequestDelegate next, ILogger<PipelineTimerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation("***********************Invoked Pipeline Timer MiddleWare***********************");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            await _next(context);
            watch.Stop();
            // Below does not work consistently especially with views. Refer documentation as modification of header after next may not work
            //context.Response.Headers.Add("X-ProcessingTime", new[] { watch.ElapsedMilliseconds.ToString() });
            // Logger is consistent
            _logger.LogInformation($"***********************Processing Time for {context.Request.Path}  { watch.ElapsedMilliseconds } ***********************");
            watch.Reset();
        }
    }
}
