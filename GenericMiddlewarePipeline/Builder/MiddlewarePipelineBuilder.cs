using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace GenericMiddlewarePipeline.Builder
{
    internal abstract class InternalMiddlewarePipelineBuilderCore<TParam> : IMiddlewarePipelineBuilderCore<TParam>
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        private Func<TParam, Task>? _coreAction;

        private IList<object> _middlewares = new List<object>();
        private IList<object[]?> _middlewareParameters = new List<object[]?>();

        public IMiddlewarePipeline<TParam> Build(MiddlewarePipelineBuildOptions? options = default)
        {
            return new InternalMiddlewarePipeline<TParam>(this, options);
        }

        public IMiddlewarePipeline<TParam> BuildLinked(MiddlewarePipelineBuildOptions? options = default)
        {
            return new InternalLinkedMiddlewarePipeline<TParam>(this, options);
        }

        public Func<TParam, Task> BuildRunAction(MiddlewarePipelineBuildOptions? options = default)
        {
            return InternalMiddlewarePipelineBuilderHelper.BuildRunAction(_coreAction, _middlewares, _middlewareParameters, options);
        }

        protected void InternalSetCoreAction(Func<TParam, Task> action)
        {
            if (action == _coreAction)
                return;

            var oldItem = _coreAction;

            _coreAction = action;

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs
            (
                action: NotifyCollectionChangedAction.Replace,
                newItem: _coreAction,
                oldItem: oldItem
            ));
        }

        protected void InternalRemove(object middleware)
        {
            for (var i = _middlewares.Count - 1; i >= 0; i--)
            {
                if (_middlewares[i] == middleware)
                {
                    _middlewares.RemoveAt(i);
                    _middlewareParameters.RemoveAt(i);

                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs
                    (
                        action: NotifyCollectionChangedAction.Remove,
                        changedItem: middleware
                    ));
                }
            }
        }

        protected void InternalAppend(object middleware, object[]? parameters)
        {
            _middlewares.Add(middleware);
            _middlewareParameters.Add(parameters);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs
            (
                action: NotifyCollectionChangedAction.Add,
                changedItem: middleware
            ));
        }

        protected void InternalPrepend(object middleware, object[]? parameters)
        {
            _middlewares.Insert(0, middleware);
            _middlewareParameters.Insert(0, parameters);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs
            (
                action: NotifyCollectionChangedAction.Add,
                changedItem: middleware
            ));
        }

        protected void InternalAfter(object middleware, object refMiddleware, object[]? parameters)
        {
            for (var i = _middlewares.Count - 1; i >= 0; i--)
            {
                if (_middlewares[i] == refMiddleware)
                {
                    if (i + 1 >= _middlewares.Count)
                    {
                        _middlewares.Add(middleware);
                        _middlewareParameters.Add(parameters);
                    }
                    else
                    {
                        _middlewares.Insert(i + 1, middleware);
                        _middlewareParameters.Insert(i + 1, parameters);
                    }

                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs
                    (
                        action: NotifyCollectionChangedAction.Add,
                        changedItem: middleware
                    ));

                    return;
                }
            }
            throw new ArgumentException(nameof(refMiddleware));
        }

        protected void InternalBefore(object middleware, object refMiddleware, object[]? parameters)
        {
            for (var i = 0; i < _middlewareParameters.Count; i++)
            {
                if (_middlewares[i] == refMiddleware)
                {
                    _middlewares.Insert(i, middleware);
                    _middlewareParameters.Insert(i, parameters);

                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs
                    (
                        action: NotifyCollectionChangedAction.Add,
                        changedItem: middleware
                    ));

                    return;
                }
            }
            throw new ArgumentException(nameof(refMiddleware));
        }
    }

    internal class InternalMiddlewarePipelineBuilder<TParam> : InternalMiddlewarePipelineBuilderCore<TParam>, IMiddlewarePipelineBuilder<TParam>
    {
        public IMiddlewarePipelineBuilder<TParam> UseAsCore(Action<TParam> action)
        {
            return UseAsCore(async param =>
            {
                action?.Invoke(param);
                await Task.Yield();
            });
        }

        public IMiddlewarePipelineBuilder<TParam> UseAsCore(Func<TParam, Task> action)
        {
            InternalSetCoreAction(action);
            return this;
        }

        public IMiddlewarePipelineBuilder<TParam> Use(Func<TParam, Func<TParam, Task>, Task> middleware)
        {
            InternalAppend(middleware, default);
            return this;
        }

        public IMiddlewarePipelineBuilder<TParam> Use<TMiddleware>()
        {
            return Use(typeof(TMiddleware));
        }

        public IMiddlewarePipelineBuilder<TParam> Use(Type middleware)
        {
            InternalAppend(middleware, default);
            return this;
        }

        public IMiddlewarePipelineBuilder<TParam> Use<TMiddleware>(params object[] parameters)
        {
            return Use(typeof(TMiddleware), parameters);
        }

        public IMiddlewarePipelineBuilder<TParam> Use(Type middleware, params object[] parameters)
        {
            InternalAppend(middleware, parameters);
            return this;
        }
    }

    internal class InternalExtendedMiddlewarePipelineBuilder<TParam> : InternalMiddlewarePipelineBuilderCore<TParam>, IExtendedMiddlewarePipelineBuilder<TParam>
    {
        public IExtendedMiddlewarePipelineBuilder<TParam> UseAsCore(Action<TParam> action)
        {
            return UseAsCore(async param =>
            {
                action?.Invoke(param);
                await Task.Yield();
            });
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> UseAsCore(Func<TParam, Task> action)
        {
            InternalSetCoreAction(action);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Remove(Func<TParam, Func<TParam, Task>, Task> middleware)
        {
            InternalRemove(middleware);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Remove<TMiddleware>()
        {
            return Remove(typeof(TMiddleware));
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Remove(Type middleware)
        {
            InternalRemove(middleware);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Append(Func<TParam, Func<TParam, Task>, Task> middleware)
        {
            InternalAppend(middleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Append<TMiddleware>()
        {
            return Append(typeof(TMiddleware));
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Append(Type middleware)
        {
            InternalAppend(middleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Append<TMiddleware>(params object[] parameters)
        {
            return Append(typeof(TMiddleware), parameters);
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Append(Type middleware, params object[] parameters)
        {
            InternalAppend(middleware, parameters);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Prepend(Func<TParam, Func<TParam, Task>, Task> middleware)
        {
            InternalPrepend(middleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Prepend<TMiddleware>()
        {
            return Prepend(typeof(TMiddleware));
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Prepend(Type middleware)
        {
            InternalPrepend(middleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Prepend<TMiddleware>(params object[] parameters)
        {
            return Prepend(typeof(TMiddleware), parameters);
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Prepend(Type middleware, params object[] parameters)
        {
            InternalPrepend(middleware, parameters);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> After(Func<TParam, Func<TParam, Task>, Task> middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware)
        {
            InternalAfter(middleware, refMiddleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> After(Func<TParam, Func<TParam, Task>, Task> middleware, Type refMiddleware)
        {
            InternalAfter(middleware, refMiddleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> After<TMiddleware>(Func<TParam, Func<TParam, Task>, Task> refMiddleware)
        {
            return After(typeof(TMiddleware), refMiddleware);
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> After<TMiddleware>(Type refMiddleware)
        {
            return After(typeof(TMiddleware), refMiddleware);
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> After(Type middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware)
        {
            InternalAfter(middleware, refMiddleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> After(Type middleware, Type refMiddleware)
        {
            InternalAfter(middleware, refMiddleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> After<TMiddleware>(Func<TParam, Func<TParam, Task>, Task> refMiddleware, params object[] parameters)
        {
            return After(typeof(TMiddleware), refMiddleware, parameters);
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> After<TMiddleware>(Type refMiddleware, params object[] parameters)
        {
            return After(typeof(TMiddleware), refMiddleware, parameters);
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> After(Type middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware, params object[] parameters)
        {
            InternalAfter(middleware, refMiddleware, parameters);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> After(Type middleware, Type refMiddleware, params object[] parameters)
        {
            InternalAfter(middleware, refMiddleware, parameters);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Before(Func<TParam, Func<TParam, Task>, Task> middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware)
        {
            InternalBefore(middleware, refMiddleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Before(Func<TParam, Func<TParam, Task>, Task> middleware, Type refMiddleware)
        {
            InternalBefore(middleware, refMiddleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Before<TMiddleware>(Func<TParam, Func<TParam, Task>, Task> refMiddleware)
        {
            return Before(typeof(TMiddleware), refMiddleware);
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Before<TMiddleware>(Type refMiddleware)
        {
            return Before(typeof(TMiddleware), refMiddleware);
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Before(Type middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware)
        {
            InternalBefore(middleware, refMiddleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Before(Type middleware, Type refMiddleware)
        {
            InternalBefore(middleware, refMiddleware, default);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Before<TMiddleware>(Func<TParam, Func<TParam, Task>, Task> refMiddleware, params object[] parameters)
        {
            return Before(typeof(TMiddleware), refMiddleware, parameters);
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Before<TMiddleware>(Type refMiddleware, params object[] parameters)
        {
            return Before(typeof(TMiddleware), refMiddleware, parameters);
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Before(Type middleware, Func<TParam, Func<TParam, Task>, Task> refMiddleware, params object[] parameters)
        {
            InternalBefore(middleware, refMiddleware, parameters);
            return this;
        }

        public IExtendedMiddlewarePipelineBuilder<TParam> Before(Type middleware, Type refMiddleware, params object[] parameters)
        {
            InternalBefore(middleware, refMiddleware, parameters);
            return this;
        }
    }
}
