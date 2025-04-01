using BlazorServiceWorkerCultureTest.FS;
using SpawnDev.BlazorJS;

namespace BlazorServiceWorkerCultureTest.Services
{
    public class IDBFSService : IAsyncBackgroundService
    {
        Task? _Ready = null;
        public Task Ready => _Ready ??= InitAsync();
        const string DBFolder = "/data";
        BlazorJSRuntime JS;
        public bool IsSupported { get; private set; }
        WASMFileSystem FS;
        public IDBFSService(BlazorJSRuntime js)
        {
            JS = js;
            using var idbfs = JS.Get<JSObject>("Blazor.runtime.Module.FS.filesystems.IDBFS");
            IsSupported = idbfs != null;
            if (!IsSupported)
            {
                JS.LogWarn("IDBFSService is loaded but 'Blazor.runtime.Module.FS.filesystems.IDBFS' is not set. The setting '<EmccExtraLDFlags>-lidbfs.js</EmccExtraLDFlags>' should be added the Blazor .csproj");
                return;
            }
            FS = WASMFileSystem.GetWASMFileSystem();
        }
        async Task InitAsync()
        {
            if (!IsSupported)
            {
                JS.Log("IDBFS unsupported");
                return;
            }
            FS.MkDir(DBFolder);
            var ret = FS.MountTest(FFFSType.IDBFS, new FSMountOptions { }, DBFolder);
            JS.Log("_MountTest", ret);
            JS.Set("_MountTest", ret);
            await FS.SyncFS(true);
            JS.Log("IDBFS MountFinished");
        }
        public async Task Sync(bool populate = false)
        {
            await FS.SyncFS(populate);
        }
    }
}
