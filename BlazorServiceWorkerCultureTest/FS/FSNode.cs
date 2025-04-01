using Microsoft.JSInterop;
using SpawnDev.BlazorJS;

namespace BlazorServiceWorkerCultureTest.FS
{
    public class FSNode : JSObject
    {
        ///<inhritdoc/>
        public FSNode(IJSInProcessObjectReference _ref) : base(_ref) { }
        public string Name => JSRef!.Get<string>("name");
        public bool IsFolder => JSRef!.Get<bool>("isFolder");
        public bool IsDevice => JSRef!.Get<bool>("isDevice");
    }
}
