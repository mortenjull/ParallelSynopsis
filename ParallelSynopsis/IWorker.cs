using System.Threading;

namespace ParallelSynopsis
{
    public interface IWorker
    {
        void Run(CancellationToken cancellationToken);
    }
}
