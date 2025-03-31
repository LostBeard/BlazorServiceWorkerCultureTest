using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.WebWorkers;

namespace BlazorServiceWorkerCultureTest
{
    public class ServiceWorker : ServiceWorkerEventHandler
    {
        public ServiceWorker(BlazorJSRuntime js) : base(js)
        {
        }
    }
}
