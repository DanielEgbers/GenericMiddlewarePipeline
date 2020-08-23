using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenericMiddlewarePipeline.Tests.Middlewares
{
    public class PowMiddleware
    {

        private Func<(IList<string> Log, Wrapped<long> Value), Task> _next;

        private int _exponent;

        public PowMiddleware(Func<(IList<string> Log, Wrapped<long> Value), Task> next, int exponent)
        {
            _next = next;

            _exponent = exponent;
        }

        public async Task InvokeAsync((IList<string> Log, Wrapped<long> Value) data)
        {
            data.Log.Add($"{nameof(PowMiddleware)}+");

            data.Value.Assign((long)Math.Pow((long)data.Value, _exponent));

            await _next(data);

            data.Log.Add($"{nameof(PowMiddleware)}-");
        }
    }
}
