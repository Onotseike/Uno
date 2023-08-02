using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoBackgroundWorker.Business
{
    public interface IBackgroundWorker
    {
        event EventHandler WorkerStopped;

        Func<Task> BackgroundWork { get; }

        void StartWorker(Func<Task> backgroundWork);

        void StopWorker();

        Task<bool> IsDataAvailableToSync();
    }
}
