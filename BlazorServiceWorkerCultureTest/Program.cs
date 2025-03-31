using BlazorServiceWorkerCultureTest;
using BlazorServiceWorkerCultureTest.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.JSObjects;
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
    ImportServiceWorkerAssets = true,
    // optionally set service worker register options
    Options = new ServiceWorkerRegistrationOptions
    {
        UpdateViaCache = "none",
    }
});

builder.Services.AddSingleton<ServiceWorkerUpdateWatchService>();

// Create host
WebAssemblyHost host = builder.Build();
IJSRuntime jsRuntime = host.Services.GetRequiredService<IJSRuntime>();

// Setting culture of the application
//await CultureFunctions.SetCulture(jsRuntime);

// Start up Blazor
await host.BlazorJSRunAsync();
