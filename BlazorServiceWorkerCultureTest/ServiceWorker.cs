using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.WebWorkers;

namespace BlazorServiceWorkerCultureTest
{
    public class ServiceWorker : ServiceWorkerEventHandler
    {
        WebWorkerService WebWorkerService;
        public ServiceWorker(BlazorJSRuntime js, WebWorkerService webWorkerService) : base(js)
        {
            // service worker code ....
            WebWorkerService = webWorkerService;
        }
        protected override async Task ServiceWorker_OnPushAsync(PushEvent e)
        {
            JS.Log("ServiceWorker_OnPushAsync", e);
            // demo payload for example
            // normally info is extracted from the event's PushMessageData at e.Data
            // Example read push message data as a Json object - var jsonObject = e.Data.Json<DataObjectType>();
            var payload = new
            {
                Message = "Waether Alert.",
                Url = "/weather",
            };
            await ServiceWorkerThis!.Registration.ShowNotification("Notification Test", new ShowNotificationsOptions
            {
                Body = payload.Message,
                Icon = "icon-512.png",
                Data = new { Url = payload.Url },
                Actions = new[] { new ShowNotificationsOptionAction { Action = "open", Title = "Open now" } },
            });
        }
        protected override async Task ServiceWorker_OnNotificationClickAsync(NotificationEvent e)
        {
            try
            {
                var actionChosen = e.Action;
                JS.Log("ServiceWorker_OnNotificationClickAsync", actionChosen);
                switch (e.Action)
                {
                    case "open":
                        {
                            using var notification = e.Notification;
                            notification.Close();
                            var windowClients = await ServiceWorkerThis!.Clients.MatchAll(new ClientsMatchAllOptions { Type = "window", IncludeUncontrolled = true });
                            // for an example of communicating with open windows, use WebWorkerService to call JS.Log in all open windows
                            var windows = WebWorkerService.Instances.Where(o => o.Info.Scope == GlobalScope.Window).ToList();
                            foreach (var window in windows)
                            {
                                // this will use the BlazorJSRuntime service in the window's scope to call js.Log
                                await window.Run<BlazorJSRuntime>(js => js.Log("notification was clicked"));
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                JS.Log();
            }
        }
        protected override async Task<Response> ServiceWorker_OnFetchAsync(FetchEvent e)
        {
            JS.Log(">> ServiceWorker_OnFetchAsync");
            var ret = await base.ServiceWorker_OnFetchAsync(e);
            JS.Log("<< ServiceWorker_OnFetchAsync");
            return ret;
        }
    }
}
