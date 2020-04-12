using System.Threading.Tasks;

namespace GenericMiddlewarePipeline
{
    public interface IMiddlewarePipeline<TParam>
    {
        Task RunAsync(TParam param);
    }
}
