using BlazorServiceWorkerCultureTest;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.WebWorkers;
using ServiceWorker = BlazorServiceWorkerCultureTest.ServiceWorker;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Enable WASM Web Workers and Service Worker
builder.Services.AddBlazorJSRuntime();
builder.Services.AddWebWorkerService();
builder.Services.RegisterServiceWorker<ServiceWorker>(new ServiceWorkerConfig
{
    ImportServiceWorkerAssets = true
});

// Create host
WebAssemblyHost host = builder.Build();

// Setting culture of the application
IJSRuntime jsRuntime = host.Services.GetRequiredService<IJSRuntime>();
await CultureFunctions.SetCulture(jsRuntime);

// Start up Blazor
await host.BlazorJSRunAsync();
