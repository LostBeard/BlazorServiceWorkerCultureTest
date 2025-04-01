using Microsoft.JSInterop;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.JSObjects;

namespace BlazorServiceWorkerCultureTest.FS
{
    public class WASMFileSystem : JSObject
    {
        public Dictionary<string, JSObject> FileSystems => JSRef!.Get<Dictionary<string, JSObject>>("filesystems");
        /// <summary>
        /// Returns WASMFileSystem? from 'Blazor.runtime.Module.FS ?? Module.FS'
        /// </summary>
        /// <returns></returns>
        public static WASMFileSystem GetWASMFileSystem()
        {
            // Not sure if Blazor.runtime.Module.FS has always existed. I read Module.FS was removed and t ouse Blazor.runtime.Module.FS isntead, so check for both starting with most recent.
            var ret = JS.Get<WASMFileSystem>("Blazor.runtime.Module.FS") ?? JS.Get<WASMFileSystem>("Module.FS");
            if (ret == null) throw new Exception("Blazor.runtime.Module.FS not found");
            return ret;
        }
        /// <summary>
        /// Returns true if an entry for the specified filesystem is found in 'filesystems'.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool FileSystemExists(string type) => !JSRef!.IsUndefined($"filesystems.{type}");
        ///<inheritdoc/>
        public WASMFileSystem(IJSInProcessObjectReference _ref) : base(_ref) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="populate">true to initialize Emscripten’s file system data with the data from the file system’s persistent source, and false to save Emscripten`s file system data to the file system’s persistent source.</param>
        /// <returns></returns>
        public async Task SyncFS(bool populate)
        {
            var tcs = new TaskCompletionSource();
            using var callback = new ActionCallback<JSObject?>(err =>
            {
                if (err == null)
                {
                    tcs.TrySetResult();
                }
                else
                {
                    tcs.TrySetException(new Exception("Failed"));
                }
            });
            SyncFS(populate, callback);
            await tcs.Task;
        }
        /// <summary>
        /// Responsible for iterating and synchronizing all mounted file systems in an asynchronous fashion.
        /// </summary>
        /// <param name="populate">true to initialize Emscripten’s file system data with the data from the file system’s persistent source, and false to save Emscripten`s file system data to the file system’s persistent source.</param>
        /// <param name="callback">A notification callback function that is invoked on completion of the synchronization. If an error occurred, it will be provided as a parameter to this function.</param>
        public void SyncFS(bool populate, ActionCallback<JSObject?> callback) => JSRef!.CallVoid("syncfs", populate, callback);
        /// <summary>
        /// Mounts the filesytem at the given mountpoint
        /// </summary>
        /// <param name="fsType"></param>
        /// <param name="options"></param>
        /// <param name="mountPoint"></param>
        /// <returns></returns>
        public FSNode? Mount(JSObject fsType, FSMountOptions options, string mountPoint) => JSRef!.Call<FSNode?>("mount", fsType, options, mountPoint);
        /// <summary>
        /// Mounts the filesytem at the given mountpoint
        /// </summary>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <param name="mountPoint"></param>
        /// <returns></returns>
        public FSNode? Mount(string type, FSMountOptions options, string mountPoint)
        {
            using var fsType = JS.Get<JSObject>($"Blazor.runtime.Module.FS.filesystems.{type}");
            return fsType == null ? null : Mount(fsType, options, mountPoint);
        }
        /// <summary>
        /// Use to unmount a mounted filesystem
        /// </summary>
        /// <param name="mountPoint"></param>
        /// <returns></returns>
        public Task<bool> Unmount(string mountPoint) => JSRef!.CallAsync<bool>("unmount", mountPoint);
        /// <summary>
        /// Creates a new directory node in the file system.
        /// </summary>
        /// <param name="path"></param>
        public void MkDir(string path) => JSRef!.CallVoid("mkdir", path);
        /// <summary>
        /// Removes an empty directory located at path.
        /// </summary>
        /// <param name="path"></param>
        public void RmDir(string path) => JSRef!.CallVoid("rmdir", path);
        /// <summary>
        /// Unlinks the node at path.<br/>
        /// This removes a name from the file system. If that name was the last link to a file (and no processes have the file open) the file is deleted.
        /// </summary>
        /// <param name="path"></param>
        public void Unlink(string path) => JSRef!.CallVoid("unlink", path);
        #region FileSystemMethods
        /// <summary>
        /// Rename a file or directory.
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public Task<bool> Rename(string oldPath, string newPath) => JSRef!.CallAsync<bool>("rename", oldPath, newPath);
        // Read
        /// <summary>
        /// Read data from ffmpeg.wasm.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="encoding">File content encoding, supports two encodings: - utf8: read file as text file, return data in string type. - binary: read file as binary file, return data in Uint8Array type. Default Value binary</param>
        /// <returns></returns>
        public Task<T> ReadFile<T>(string path, string encoding) => JSRef!.CallAsync<T>("readFile", path, encoding);
        /// <summary>
        /// Read data from ffmpeg.wasm.
        /// </summary>
        public Task<string> ReadFileUTF8(string path) => ReadFile<string>(path, "utf8");
        /// <summary>
        /// Read data from ffmpeg.wasm.
        /// </summary>
        public Task<Uint8Array> ReadFile(string path) => ReadFile<Uint8Array>(path, "binary");
        /// <summary>
        /// Read data from ffmpeg.wasm.
        /// </summary>
        public Task<Uint8Array> ReadFileUint8Array(string path) => ReadFile<Uint8Array>(path, "binary");
        /// <summary>
        /// Read data from ffmpeg.wasm.
        /// </summary>
        public Task<byte[]> ReadFileBytes(string path) => ReadFile<byte[]>(path, "binary");
        // Write
        /// <summary>
        /// Write data to ffmpeg.wasm.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<bool> WriteFile(string path, string data) => JSRef!.CallAsync<bool>("writeFile", path, data);
        /// <summary>
        /// Write data to ffmpeg.wasm.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<bool> WriteFile(string path, Uint8Array data) => JSRef!.CallAsync<bool>("writeFile", path, data);
        /// <summary>
        /// Write data to ffmpeg.wasm.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<bool> WriteFile(string path, byte[] data) => JSRef!.CallAsync<bool>("writeFile", path, data);
        #endregion
    }
}
