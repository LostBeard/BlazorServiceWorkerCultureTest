using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.WebWorkers;

namespace BlazorServiceWorkerCultureTest
{
    public class ServiceWorker : ServiceWorkerEventHandler
    {
        public ServiceWorker(BlazorJSRuntime js) : base(js)
        {
            // service worker code ....
        }
        protected override async Task ServiceWorker_OnPushAsync(PushEvent e)
        {
            
        }
    }
}
