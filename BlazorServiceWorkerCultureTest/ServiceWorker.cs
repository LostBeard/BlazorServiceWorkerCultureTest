using Microsoft.AspNetCore.Components;
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
            // Example read push message data as a Json object
            //var payload = e.Data.Json<Payload>();
            var payload = new Payload
            {
                Message = "Waether Alert.",
                Url = "/weather",
            };
            await ServiceWorkerThis!.Registration.ShowNotification("Notification Test", new ShowNotificationsOptions
            {
                Body = payload.Message,
                Icon = "icon-512.png",
                Data = payload,
                Actions = new[] { new ShowNotificationsOptionAction { Action = "open", Title = "Open now" } },
            });
        }
        protected override async Task ServiceWorker_OnNotificationClickAsync(NotificationEvent e)
        {
            try
            {
                // handle the selected action
                var actionChosen = e.Action;
                JS.Log("ServiceWorker_OnNotificationClickAsync", actionChosen);
                switch (e.Action)
                {
                    case "open":
                        {
                            // get the notification
                            using var notification = e.Notification;

                            // access the notification data set in the push event handler
                            var payload = notification.DataAs<Payload>();

                            // close the notification
                            notification.Close();


                            // work with windows the way you would in Javascript
                            var windowClients = await ServiceWorkerThis!.Clients.MatchAll(new ClientsMatchAllOptions { Type = "window", IncludeUncontrolled = true });
                            // bring an existing window to focus, open a new window, etc

                            // OR

                            // WebWorkerService provides a way to directly call any service running in any in winow running your app
                            // for an example use WebWorkerService to call JS.Log in all open windows
                            var windows = WebWorkerService.Instances.Where(o => o.Info.Scope == GlobalScope.Window).ToList();
                            // find a window to pass payload to
                            var windowFirst = windows.FirstOrDefault();
                            // make the payload url relative
                            // an absolute url (starts with /) will fail if your Blazor app has a non-root baseHref
                            var payloadUrl = payload.Url.TrimStart('/');
                            // if there is no window that meets your needs you can open a new one (mostly only works in the notification click event when in a service worker)
                            if (windowFirst == null)
                            {
                                try
                                {
                                    // open a new window at the payload page
                                    windowFirst = await WebWorkerService.OpenWindow(payloadUrl);
                                    // windowFirst may be null here
                                }
                                catch (Exception ex)
                                {
                                    JS.Log("WebWorkerService.OpenWindow error", ex.Message);
                                    JS.Log(ex.StackTrace);
                                    // create may fail with or without an error
                                }
                                if (windowFirst != null)
                                {
                                    JS.Log("windowFirst != null");
                                }
                                else
                                {
                                    JS.Log("windowFirst == null");
                                }
                            }
                            else
                            {
                                // send the existing window to the payload page
                                await windowFirst.Run<NavigationManager>(navigationManager => navigationManager.NavigateTo(payloadUrl, false, false));
                            }
                            if (windowFirst != null)
                            {
                                // can hand off the paylaod to the window if desired
                                await windowFirst.Run<ServiceWorker>(sw => sw.HandlePushAction(payload));
                            }
                            else
                            {
                                // could not hand off the payload to a window...
                                // could store it and have a service ;ook for missed payoads on startup if needed
                                JS.Log("No window available");
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                JS.Log("ServiceWorker_OnNotificationClickAsync error", ex.Message);
                JS.Log(ex.StackTrace);
            }
        }
        /// <summary>
        /// This method will be called by this service running in the service worker scope whe na push notification action is chosen
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        async Task HandlePushAction(Payload payload)
        {
            // this method will be called by the service worker but will wun in the window scope
            JS.Log($"{JS.GlobalScope.ToString()} {JS.InstanceId} received the push notification payload", payload);
            // could fire an event pages and components could listen to here
        }
    }
}
