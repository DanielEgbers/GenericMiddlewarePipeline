using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GenericMiddlewarePipeline.Builder
{
    internal static class InternalMiddlewarePipelineBuilderHelper
    {
        public static Func<TParam, Task> BuildRunAction<TParam>(Func<TParam, Task>? coreAction, IList<object> middlewares, IList<object[]?> middlewareParameters, MiddlewarePipelineBuildOptions? options)
        {
            var invokeAction = coreAction ?? (async param => await Task.Yield());

            for (var i = middlewares.Count - 1; i >= 0; i--)
            {
                invokeAction = BuildInvokeAction(middlewares[i], middlewareParameters[i], invokeAction, options) ?? invokeAction;
            }

            return invokeAction;
        }

        private static Func<TParam, Task>? BuildInvokeAction<TParam>(object middleware, object[]? middlewareParameters, Func<TParam, Task> next, MiddlewarePipelineBuildOptions? options)
        {
            if (middleware is Func<TParam, Func<TParam, Task>, Task> action)
            {
                return async param => await action.Invoke(param, next);
            }
            else if (middleware is Type type)
            {
                var invoke = GetInvokeMethod<TParam>(type, out var invokeError);
                if (invoke == default || !string.IsNullOrWhiteSpace(invokeError))
                {
                    if (options?.IgnoreInvalidMiddlewares == true)
                        return default;
                    else
                        throw new InvalidMiddlewareException(invokeError ?? string.Empty);
                }

                var instance = BuildInstance(type, middlewareParameters, new object[] { next }, out var instanceError);
                if (instance == default || !string.IsNullOrWhiteSpace(instanceError))
                {
                    if (options?.IgnoreInvalidMiddlewares == true)
                        return default;
                    else
                        throw new InvalidMiddlewareException(instanceError ?? string.Empty);
                }

                return async param => await (Task)invoke.Invoke(instance, new object[] { param! });
            }
            else
            {
                throw new NotSupportedException(middleware?.GetType().FullName);
            }
        }

        private static MethodInfo? GetInvokeMethod<TParam>(Type type, out string? error)
        {
            const string MethodNameInvoke = "Invoke";
            const string MethodNameInvokeAsync = MethodNameInvoke + "Async";

            error = default;

            var paramTypes = new[] { typeof(TParam) };

            var invoke =
                type.GetMethod(MethodNameInvokeAsync, paramTypes) ??
                type.GetMethod(MethodNameInvoke, paramTypes)
                ;

            if (invoke == default)
                error = $"No public '{MethodNameInvoke}' or '{MethodNameInvokeAsync}' method found for middleware of type '{type.FullName}'";

            if (invoke?.ReturnType != typeof(Task))
                error = $"'{MethodNameInvoke}' or '{MethodNameInvokeAsync}' does not return an object of type '{nameof(Task)}'";

            return invoke;
        }

        private static object? BuildInstance(Type type, object[]? paramValues, object[] internalParamValues, out string? error)
        {
            error = default;

            object? InternalBuildInstance()
            {
                var internalParamTypes = internalParamValues.Select(p => p.GetType()).ToArray();

                if (paramValues != default)
                {
                    if (paramValues.Any(p => p == default))
                        return default;

                    var paramTypes = paramValues.Select(p => p.GetType()).ToArray();

                    {
                        var constructor = type.GetConstructor(internalParamTypes.Concat(paramTypes).ToArray());
                        var instance = constructor?.Invoke(internalParamValues.Concat(paramValues).ToArray());
                        if (instance != default)
                            return instance;
                    }

                    {
                        var constructor = type.GetConstructor(paramTypes.Concat(internalParamTypes).ToArray());
                        var instance = constructor?.Invoke(paramValues.Concat(internalParamValues).ToArray());
                        if (instance != default)
                            return instance;
                    }

                    return default;
                }
                else
                {
                    var constructor = type.GetConstructor(internalParamTypes);
                    return constructor?.Invoke(internalParamValues);
                }
            }

            var instance = InternalBuildInstance();

            if (instance == default)
                error = $"A suitable constructor for type '{type.FullName}' could not be located";

            return instance;
        }
    }
}
