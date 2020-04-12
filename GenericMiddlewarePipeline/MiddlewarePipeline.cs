using GenericMiddlewarePipeline.Builder;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace GenericMiddlewarePipeline
{
    public static class MiddlewarePipeline
    {
        public static IMiddlewarePipelineBuilder<TParam> New<TParam>()
        {
            return new InternalMiddlewarePipelineBuilder<TParam>();
        }

        public static IExtendedMiddlewarePipelineBuilder<TParam> NewExtended<TParam>()
        {
            return new InternalExtendedMiddlewarePipelineBuilder<TParam>();
        }
    }

    internal class InternalMiddlewarePipeline<TParam> : IMiddlewarePipeline<TParam>
    {
        private Func<TParam, Task> _runAction;

        public InternalMiddlewarePipeline(InternalMiddlewarePipelineBuilderCore<TParam> builder, MiddlewarePipelineBuildOptions? buildOptions)
        {
            _runAction = builder.BuildRunAction(buildOptions);
        }

        public async Task RunAsync(TParam param)
        {
            await _runAction.Invoke(param);
        }
    }

    internal class InternalLinkedMiddlewarePipeline<TParam> : IMiddlewarePipeline<TParam>
    {
        private InternalMiddlewarePipelineBuilderCore<TParam> _builder;
        private MiddlewarePipelineBuildOptions? _buildOptions;

        private bool _isRunActionPrepared = false;
        private Func<TParam, Task>? _runAction;

        public InternalLinkedMiddlewarePipeline(InternalMiddlewarePipelineBuilderCore<TParam> builder, MiddlewarePipelineBuildOptions? buildOptions)
        {
            _builder = builder;
            _buildOptions = buildOptions;

            PrepareRunAction();

            _builder.CollectionChanged += InvalidateRunActionOnBuilderChange;
        }

        ~InternalLinkedMiddlewarePipeline()
        {
            _builder.CollectionChanged -= InvalidateRunActionOnBuilderChange;
        }

        public async Task RunAsync(TParam param)
        {
            PrepareRunAction();
            if (_runAction == default) return;
            await _runAction.Invoke(param);
        }

        private void PrepareRunAction()
        {
            if (_isRunActionPrepared) return;
            _runAction = _builder.BuildRunAction(_buildOptions);
            _isRunActionPrepared = true;
        }

        private void InvalidateRunActionOnBuilderChange(object sender, NotifyCollectionChangedEventArgs args)
        {
            _isRunActionPrepared = false;
        }
    }
}
