using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenericMiddlewarePipeline.Tests.Middlewares
{
    public class LogMiddlewareWithParams
    {
        private Func<IList<string>, Task> _next;

        private int _paramI;
        private string _paramS;
        private IList<int> _paramLI;
        private IReadOnlyList<string> _paramLS;
        private bool[] _paramAB;

        public LogMiddlewareWithParams(Func<IList<string>, Task> next, int paramI, string paramS, IList<int> paramLI, IReadOnlyList<string> paramLS, bool[] paramAB)
        {
            _next = next;

            _paramI = paramI;
            _paramS = paramS;
            _paramLI = paramLI;
            _paramLS = paramLS;
            _paramAB = paramAB;
        }

        public async Task InvokeAsync(IList<string> log)
        {
            log.Add($"{nameof(LogMiddlewareWithParams)}+");
            log.Add($"Params: {_paramI}, {_paramS}, {_paramLI}, {_paramLS}, {_paramAB}");
            await _next(log);
            log.Add($"{nameof(LogMiddlewareWithParams)}-");
        }
    }

    public class LogMiddlewareWithParamsNextAsLast
    {
        private Func<IList<string>, Task> _next;

        private int _paramI;
        private int _paramI2;
        private int _paramI3;

        public LogMiddlewareWithParamsNextAsLast(int paramI, int paramI2, int paramI3, Func<IList<string>, Task> next)
        {
            _next = next;

            _paramI = paramI;
            _paramI2 = paramI2;
            _paramI3 = paramI3;
        }

        public async Task InvokeAsync(IList<string> log)
        {
            log.Add($"{nameof(LogMiddlewareWithParamsNextAsLast)}+");
            log.Add($"Params: {_paramI}, {_paramI2}, {_paramI3}");
            await _next(log);
            log.Add($"{nameof(LogMiddlewareWithParamsNextAsLast)}-");
        }
    }
}
