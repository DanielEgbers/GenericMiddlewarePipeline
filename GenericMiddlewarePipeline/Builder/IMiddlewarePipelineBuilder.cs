using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace GenericMiddlewarePipeline.Builder
{
    public interface IMiddlewarePipelineBuilderCore<TParam> : INotifyCollectionChanged
    {
        IMiddlewarePipeline<TParam> Build(MiddlewarePipelineBuildOptions? options = default);
        IMiddlewarePipeline<TParam> BuildLinked(MiddlewarePipelineBuildOptions? options = default);
    }

    public interface IMiddlewarePipelineBuilder<TParam> : IMiddlewarePipelineBuilderCore<TParam>
    {
        IMiddlewarePipelineBuilder<TParam> UseAsCore(Action<TParam> action);
        IMiddlewarePipelineBuilder<TParam> UseAsCore(Func<TParam, Task> action);

        IMiddlewarePipelineBuilder<TParam> Use(Func<TParam, Func<TParam, Task>, Task> middleware);

        IMiddlewarePipelineBuilder<TParam> Use<TMiddleware>();
        IMiddlewarePipelineBuilder<TParam> Use(Type middleware);
        IMiddlewarePipelineBuilder<TParam> Use<TMiddleware>(params object[] parameters);
        IMiddlewarePipelineBuilder<TParam> Use(Type middleware, params object[] parameters);
    }

    public interface IExtendedMiddlewarePipelineBuilder<TParam> : IMiddlewarePipelineBuilderCore<TParam>
    {
        IExtendedMiddlewarePipelineBuilder<TParam> UseAsCore(Action<TParam> action);
        IExtendedMiddlewarePipelineBuilder<TParam> UseAsCore(Func<TParam, Task> action);

        IExtendedMiddlewarePipelineBuilder<TParam> Remove(Func<TParam, Func<TParam, Task>, Task> middleware);

        IExtendedMiddlewarePipelineBuilder<TParam> Remove<TMiddleware>();
        IExtendedMiddlewarePipelineBuilder<TParam> Remove(Type middleware);

        IExtendedMiddlewarePipelineBuilder<TParam> Append(Func<TParam, Func<TParam, Task>, Task> middleware);

        IExtendedMiddlewarePipelineBuilder<TParam> Append<TMiddleware>();
        IExtendedMiddlewarePipelineBuilder<TParam> Append(Type middleware);
        IExtendedMiddlewarePipelineBuilder<TParam> Append<TMiddleware>(params object[] parameters);
        IExtendedMiddlewarePipelineBuilder<TParam> Append(Type middleware, params object[] parameters);

        IExtendedMiddlewarePipelineBuilder<TParam> Prepend(Func<TParam, Func<TParam, Task>, Task> middleware);

        IExtendedMiddlewarePipelineBuilder<TParam> Prepend<TMiddleware>();
        IExtendedMiddlewarePipelineBuilder<TParam> Prepend(Type middleware);
        IExtendedMiddlewarePipelineBuilder<TParam> Prepend<TMiddleware>(params object[] parameters);
        IExtendedMiddlewarePipelineBuilder<TParam> Prepend(Type middleware, params object[] parameters);

        IExtendedMiddlewarePipelineBuilder<TParam> After(Func<TParam, Func<TParam, Task>, Task> middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware);
        IExtendedMiddlewarePipelineBuilder<TParam> After(Func<TParam, Func<TParam, Task>, Task> middleware, Type refMiddleware);

        IExtendedMiddlewarePipelineBuilder<TParam> After<TMiddleware>(Func<TParam, Func<TParam, Task>, Task> refMiddleware);
        IExtendedMiddlewarePipelineBuilder<TParam> After<TMiddleware>(Type refMiddleware);
        IExtendedMiddlewarePipelineBuilder<TParam> After(Type middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware);
        IExtendedMiddlewarePipelineBuilder<TParam> After(Type middleware, Type refMiddleware);
        IExtendedMiddlewarePipelineBuilder<TParam> After<TMiddleware>(Func<TParam, Func<TParam, Task>, Task> refMiddleware, params object[] parameters);
        IExtendedMiddlewarePipelineBuilder<TParam> After<TMiddleware>(Type refMiddleware, params object[] parameters);
        IExtendedMiddlewarePipelineBuilder<TParam> After(Type middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware, params object[] parameters);
        IExtendedMiddlewarePipelineBuilder<TParam> After(Type middleware, Type refMiddleware, params object[] parameters);

        IExtendedMiddlewarePipelineBuilder<TParam> Before(Func<TParam, Func<TParam, Task>, Task> middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware);
        IExtendedMiddlewarePipelineBuilder<TParam> Before(Func<TParam, Func<TParam, Task>, Task> middleware, Type refMiddleware);

        IExtendedMiddlewarePipelineBuilder<TParam> Before<TMiddleware>(Func<TParam, Func<TParam, Task>, Task> refMiddleware);
        IExtendedMiddlewarePipelineBuilder<TParam> Before<TMiddleware>(Type refMiddleware);
        IExtendedMiddlewarePipelineBuilder<TParam> Before(Type middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware);
        IExtendedMiddlewarePipelineBuilder<TParam> Before(Type middleware, Type refMiddleware);
        IExtendedMiddlewarePipelineBuilder<TParam> Before<TMiddleware>(Func<TParam, Func<TParam, Task>, Task> refMiddleware, params object[] parameters);
        IExtendedMiddlewarePipelineBuilder<TParam> Before<TMiddleware>(Type refMiddleware, params object[] parameters);
        IExtendedMiddlewarePipelineBuilder<TParam> Before(Type middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware, params object[] parameters);
        IExtendedMiddlewarePipelineBuilder<TParam> Before(Type middleware, Type refMiddleware, params object[] parameters);
    }
}
