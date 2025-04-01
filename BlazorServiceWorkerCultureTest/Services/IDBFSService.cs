using BlazorServiceWorkerCultureTest.FS;
using SpawnDev.BlazorJS;

namespace BlazorServiceWorkerCultureTest.Services
{
    public class IDBFSService : IAsyncBackgroundService
    {
        // IAsyncBackgroundService services are automatically started and initialized by BlazorJSRuntime before any page loads
        // Ready will be awaited by BlazorJSRuntime before any page is laoded
        Task? _Ready = null;
        public Task Ready => _Ready ??= InitAsync();
        public bool IsSupported { get; private set; }
        const string DBFolder = "/data";
        BlazorJSRuntime JS;
        WASMFileSystem FS;
        public IDBFSService(BlazorJSRuntime js)
        {
            JS = js;
            FS = WASMFileSystem.GetWASMFileSystem();
            IsSupported = FS.FileSystemExists("IDBFS");
            if (!IsSupported)
            {
                JS.LogWarn("IDBFSService is loaded but 'Blazor.runtime.Module.FS.filesystems.IDBFS' is not set. The setting '<EmccExtraLDFlags>-lidbfs.js</EmccExtraLDFlags>' should be added the Blazor .csproj");
                return;
            }
        }
        /// <summary>
        /// This method will be ran on startup via the Ready property
        /// </summary>
        /// <returns></returns>
        async Task InitAsync()
        {
            if (!IsSupported)
            {
                JS.Log("IDBFS unsupported");
                return;
            }
            FS.MkDir(DBFolder);
            FS.Mount("IDBFS", new FSMountOptions { }, DBFolder);
            await FS.SyncFS(true);
            JS.Log("IDBFS MountFinished");
        }
        /// <summary>
        /// Synchronize the IDBFS
        /// </summary>
        /// <param name="populate"></param>
        /// <returns></returns>
        public async Task Sync(bool populate = false)
        {
            if (!IsSupported) return;
            await FS.SyncFS(populate);
        }
    }
}
