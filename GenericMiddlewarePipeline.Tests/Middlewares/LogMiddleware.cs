using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenericMiddlewarePipeline.Tests.Middlewares
{
    public class LogMiddlewareA
    {
        private Func<IList<string>, Task> _next;

        public LogMiddlewareA(Func<IList<string>, Task> next)
        {
            _next = next;
        }

        public async Task InvokeAsync(IList<string> log)
        {
            log.Add($"{nameof(LogMiddlewareA)}+");
            await _next(log);
            log.Add($"{nameof(LogMiddlewareA)}-");
        }
    }

    public class LogMiddlewareB
    {
        private Func<IList<string>, Task> _next;

        public LogMiddlewareB(Func<IList<string>, Task> next)
        {
            _next = next;
        }

        public async Task InvokeAsync(IList<string> log)
        {
            log.Add($"{nameof(LogMiddlewareB)}+");
            await _next(log);
            log.Add($"{nameof(LogMiddlewareB)}-");
        }
    }
}
