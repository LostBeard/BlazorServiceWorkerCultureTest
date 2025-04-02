using BlazorServiceWorkerCultureTest;
using BlazorServiceWorkerCultureTest.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.WebWorkers;
using File = System.IO.File;
using ServiceWorker = BlazorServiceWorkerCultureTest.ServiceWorker;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Enable WASM Web Workers and Service Worker
builder.Services.AddBlazorJSRuntime();
builder.Services.AddWebWorkerService();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var useServiceWorker = true;
if (useServiceWorker)
{
    builder.Services.RegisterServiceWorker<ServiceWorker>(new ServiceWorkerConfig
    {
        ImportServiceWorkerAssets = true,
        // optionally set service worker register options
        Options = new ServiceWorkerRegistrationOptions
        {
            UpdateViaCache = "none",
        }
    });
}
else
{
    builder.Services.UnregisterServiceWorker();
}

builder.Services.AddSingleton<ServiceWorkerUpdateWatchService>();

builder.Services.AddSingleton<IDBFSService>();

// Create host
WebAssemblyHost host = builder.Build();

// because we want to use the IDBService after it is initialized and it is an IAsyncBackgroundService
// start all background services and wait until initialization is complete (automatically done by 'host.BlazorJSRunAsync()' if not done before hand)
await host.StartBackgroundServices();

// if published, IDBFS is now mounted
var IDBFSService = host.Services.GetRequiredService<IDBFSService>();
if (IDBFSService.IsSupported)
{
    // Test
    string filename = "/data/Test.txt";

    // Create if not exists
    if (File.Exists(filename))
        Console.WriteLine("File already exists.");
    else
    {
        Console.WriteLine("Create file.");

        // Write file
        File.WriteAllText(filename, "Hello persistent filesystem.");

        await IDBFSService.Sync();
    }

    // Read
    if (File.Exists(filename))
        Console.WriteLine(File.ReadAllText(filename));
}
else
{
    // probably not published, or the project is missing the '<EmccExtraLDFlags>-lidbfs.js</EmccExtraLDFlags>' settings in the '.csproj' file
}

// Start up Blazor
await host.BlazorJSRunAsync();
