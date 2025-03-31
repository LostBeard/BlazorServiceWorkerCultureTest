using Microsoft.AspNetCore.Components;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.BlazorJSRuntimeAnyKey;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.WebWorkers;

namespace BlazorServiceWorkerCultureTest.Services
{
    public class ServiceWorkerUpdateWatchService : IBackgroundService
    {
        BlazorJSRuntime JS;
        WebWorkerService WebWorkerService;
        SpawnDev.BlazorJS.JSObjects.ServiceWorker? NewWorker;
        Navigator Navigator;
        public bool Refreshing { get; set; }
        NavigationManager NavigationManager;
        public ServiceWorkerUpdateWatchService(BlazorJSRuntime js, WebWorkerService webWorkerService, NavigationManager navigationManager)
        {
            JS = js;
            WebWorkerService = webWorkerService;
            NavigationManager = navigationManager;
            Navigator = JS.Get<Navigator>("navigator");
            // this is a IBackgroundService service and will be started in all scopes
            // makes sure to check if the code is running in a window scope and not the service worker (or other) scope
            if (JS.IsWindow)
            {
                // running in the window scope
                InstallApp();
            }
            else if (JS.ServiceWorkerThis != null)
            {
                // running in the service worker scope
                JS.ServiceWorkerThis.OnMessage += ServiceWorker_OnMessage;
            }
        }

        void ServiceWorker_OnMessage(ExtendableMessageEvent extendableMessageEvent)
        {
            using var data = extendableMessageEvent.GetData<JSObject>();
            if (data.JSRef!.TypeOf() == "string")
            {
                var message = data.JSRef!.As<string>();
                if (message == "skipWaiting")
                {
                    // JS.Log("Skip waiting recvd");
                    JS.CallVoid("self.skipWaiting");
                }
            }
        }

        void InstallApp()
        {
            using var serviceWorker = Navigator.ServiceWorker;
            if (serviceWorker == null)
            {
                JS.LogWarn("Service workers are not enabled.");
            }
            else
            {
                // WebWorkerService handles calling the service worker registration and listens for 'updatefound'
                // attach to the updatefound event to get the new worker
                WebWorkerService.OnServiceWorkerUpdateFound += WebWorkerService_OnServiceWorkerUpdateFound;
                // optionally listen for the service worker registration event
                WebWorkerService.OnServiceWorkerRegistered += WebWorkerService_OnServiceWorkerRegistered;
                // listen for the controller change event
                serviceWorker.OnControllerChange += ServiceWorker_OnControllerChange;
            }
        }

        void WebWorkerService_OnServiceWorkerRegistered(ServiceWorkerRegistration serviceWorkerRegistration)
        {
            JS.Log("Service worker registered");
        }

        public void TellServiceWorkerSkipWaiting()
        {
            if (NewWorker != null && JS.IsWindow)
            {
                NewWorker.PostMessage("skipWaiting");
            }
        }

        void ServiceWorker_OnControllerChange()
        {
            if (!Refreshing)
            {
                JS.Log("Controller change");
                Refreshing = true;
                NavigationManager.Refresh(true);
            }
        }

        private async void WebWorkerService_OnServiceWorkerUpdateFound()
        {
            if (JS.IsWindow)
            {
                JS.Log("Service worker update found");
                // check if a service worker update is waiting 
                using var registration = await JS.GetAsync<ServiceWorkerRegistration>("navigator.serviceWorker.ready");
                NewWorker = registration.Installing;
                NewWorker!.OnStateChange += NewWorker_OnStateChange;
            }

        }

        public bool InstallWaiting { get; set; } = false;
        public event Action OnServiceWorkerInstalled;
        void NewWorker_OnStateChange()
        {
            if (NewWorker == null) return;
            if (NewWorker.State == "installed" && Navigator.ServiceWorker?.Controller != null)
            {
                //using var document = JS.GetDocument();
                //var snackbar = document!.GetElementById("snackbarcontainer");
                //if (snackbar != null) snackbar.ClassName = "show";
                JS.Log("NewWorker_OnStateChange: installed");
                // fire an event that componenets can listen to, to show a snackbar
                // maube set a boolean value, or other...
                InstallWaiting = true;
                OnServiceWorkerInstalled?.Invoke();
            }
        }
    }
}
