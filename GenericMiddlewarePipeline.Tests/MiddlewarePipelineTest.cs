using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenericMiddlewarePipeline.Tests.Middlewares;
using Xunit;

namespace GenericMiddlewarePipeline.Tests
{
    public class MiddlewarePipelineTest
    {
        [Fact]
        public async Task Build_And_Run()
        {
            var expected = new[]
            {
                $"{nameof(LogMiddlewareA)}+",
                "AnonymousA+",
                $"{nameof(LogMiddlewareB)}+",
                "Core",
                $"{nameof(LogMiddlewareB)}-",
                "AnonymousA-",
                $"{nameof(LogMiddlewareA)}-",
            };

            var actual = new List<string>();

            await MiddlewarePipeline.New<IList<string>>()
                .Use<LogMiddlewareA>()
                .Use(async (log, next) =>
                {
                    log.Add("AnonymousA+");
                    await next(log);
                    log.Add("AnonymousA-");
                })
                .Use(typeof(LogMiddlewareB))
                .UseAsCore(log =>
                {
                    log.Add("Core");
                })
                .Build()
                .RunAsync(actual)
                ;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(42, 2, 3, @"(/&%$%§", new[] { 1, 2, 3 }, new[] { "A", "B", "C" }, new[] { true, true, false })]
        public async Task Build_And_Run_With_Params(int paramI, int paramI2, int paramI3, string paramS, int[] paramAI, string[] paramAS, bool[] paramAB)
        {
            var paramLI = new List<int>(paramAI);
            var paramLS = new List<string>(paramAS);

            var expected = new[]
            {
                $"{nameof(LogMiddlewareWithParams)}+",
                $"Params: {paramI}, {paramS}, {paramLI}, {paramLS}, {paramAB}",
                $"{nameof(LogMiddlewareWithParamsNextAsLast)}+",
                $"Params: {paramI}, {paramI2}, {paramI3}",
                $"{nameof(LogMiddlewareWithParamsNextAsLast)}-",
                $"{nameof(LogMiddlewareWithParams)}-",
            };

            var actual = new List<string>();

            await MiddlewarePipeline.New<IList<string>>()
                .Use<LogMiddlewareWithParams>(paramI, paramS, paramLI, paramLS, paramAB)
                .Use<LogMiddlewareWithParamsNextAsLast>(paramI, paramI2, paramI3)
                .Build()
                .RunAsync(actual)
                ;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(2, 2)]
        [InlineData(2, 4)]
        public async Task Build_And_Run_With_Multiple_Middleware_Of_Same_Type(long @base, int exponent)
        {
            var expected = new[]
            {
                $"{@base}",
                $"{nameof(PowMiddleware)}+",
                $"{nameof(PowMiddleware)}+",
                $"{nameof(PowMiddleware)}+",
                $"{nameof(PowMiddleware)}+",
                $"{(long)Math.Pow(Math.Pow(Math.Pow(@base, exponent), exponent + 1), exponent)}",
                $"{nameof(PowMiddleware)}-",
                $"{nameof(PowMiddleware)}-",
                $"{nameof(PowMiddleware)}-",
                $"{nameof(PowMiddleware)}-",
                $"{(long)Math.Pow(Math.Pow(Math.Pow(@base, exponent), exponent + 1), exponent)}",
            };

            var actual = new List<string>();

            var data = (actual, (Wrapped<long>)@base);

            await MiddlewarePipeline.New<(IList<string> Log, Wrapped<long> Value)>()
                .Use(async (data, next) =>
                {
                    data.Log.Add($"{data.Value}");
                    await next(data);
                    data.Log.Add($"{data.Value}");
                })
                .Use<PowMiddleware>(exponent)
                .Use<PowMiddleware>(exponent + 1)
                .Use<PowMiddleware>(exponent)
                .Use<PowMiddleware>(1)
                .UseAsCore(data =>
                {
                    data.Log.Add($"{data.Value}");
                })
                .Build()
                .RunAsync(data)
                ;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Build_Linked_And_Run(bool linked)
        {
            var builder = MiddlewarePipeline.New<IList<string>>()
                .Use<LogMiddlewareA>()
                .Use<LogMiddlewareB>()
                .UseAsCore(log =>
                {
                    log.Add("CoreA");
                })
                ;

            var pipeline = linked ? builder.BuildLinked() : builder.Build();

            {
                var expected = new[]
                {
                        $"{nameof(LogMiddlewareA)}+",
                        $"{nameof(LogMiddlewareB)}+",
                        "CoreA",
                        $"{nameof(LogMiddlewareB)}-",
                        $"{nameof(LogMiddlewareA)}-",
                    };

                var actual = new List<string>();

                await pipeline.RunAsync(actual);

                Assert.Equal(expected, actual);
            }

            builder.UseAsCore(log =>
            {
                log.Add("CoreB");
            });

            {
                var expected = new[]
                {
                        $"{nameof(LogMiddlewareA)}+",
                        $"{nameof(LogMiddlewareB)}+",
                        linked ? "CoreB" : "CoreA",
                        $"{nameof(LogMiddlewareB)}-",
                        $"{nameof(LogMiddlewareA)}-",
                    };

                var actual = new List<string>();

                await pipeline.RunAsync(actual);

                Assert.Equal(expected, actual);
            }
        }
    }
}
