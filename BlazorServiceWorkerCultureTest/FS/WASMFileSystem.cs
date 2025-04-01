using Microsoft.JSInterop;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.JSObjects;

namespace BlazorServiceWorkerCultureTest.FS
{
    public class WASMFileSystem : JSObject
    {
        static string[] VariableName = new string[] { "Blazor.runtime.Module.FS" };
        public Dictionary<string, JSObject> FileSystems => JSRef!.Get<Dictionary<string, JSObject>>("filesystems");
        public static WASMFileSystem GetWASMFileSystem()
        {
            WASMFileSystem? ret = null;
            foreach (var name in VariableName)
            {
                ret = JS.Get<WASMFileSystem>(name);
                if (ret != null) return ret;
            }
            throw new Exception("Blazor.runtime.Module.FS not found");
        }
        ///<inheritdoc/>
        public WASMFileSystem(IJSInProcessObjectReference _ref) : base(_ref) { }
        //public async Task Mount(string type, WASMFileSystemMountOptions options, string mountPoint)
        //{
        //    using var fsType = JS.Get<JSObject>($"Blazor.runtime.Module.FS.filesystems.{type}");
        //    if (fsType == null)
        //    {
        //        throw new Exception($"Filesystem type not found: {type}");
        //    }
        //}
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
        /// 
        /// </summary>
        /// <param name="populate">true to initialize Emscripten’s file system data with the data from the file system’s persistent source, and false to save Emscripten`s file system data to the file system’s persistent source.</param>
        /// <param name="callback">A notification callback function that is invoked on completion of the synchronization. If an error occurred, it will be provided as a parameter to this function.</param>
        public void SyncFS(bool populate, ActionCallback<JSObject?> callback) => JSRef!.CallVoid("syncfs", populate, callback);

        public JSObject? MountTest(JSObject fsType, FSMountOptions options, string mountPoint) => JSRef!.Call<JSObject?>("mount", fsType, options, mountPoint);

        /// <summary>
        /// Allows mounting of WORKERFS in supported builds of ffmpeg.wasm
        /// </summary>
        /// <param name="fsType"></param>
        /// <param name="options"></param>
        /// <param name="mountPoint"></param>
        /// <returns></returns>
        public Task<bool> Mount(EnumString<FFFSType> fsType, FSMountOptions options, string mountPoint) => JSRef!.CallAsync<bool>("mount", fsType, options, mountPoint);
        /// <summary>
        /// Use to unmount a mounted filesystem
        /// </summary>
        /// <param name="mountPoint"></param>
        /// <returns></returns>
        public Task<bool> Unmount(string mountPoint) => JSRef!.CallAsync<bool>("unmount", mountPoint);
        /// <summary>
        /// Convenience function to mount WORKERFS in supported builds of ffmpeg.wasm
        /// </summary>
        /// <param name="mountPoint"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Task<bool> MountWorkerFS(string mountPoint, FSMountWorkerFSOptions options) => Mount(FFFSType.WORKERFS, options, mountPoint);
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
        ///// <summary>
        ///// Create a directory.
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //public Task<bool> CreateDir(string path) => JSRef!.CallAsync<bool>("createDir", path);
        ///// <summary>
        ///// Delete an empty directory.
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //public Task<bool> DeleteDir(string path) => JSRef!.CallAsync<bool>("deleteDir", path);
        ///// <summary>
        ///// List directory contents.
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //public Task<FSNode[]> ListDir(string path) => JSRef!.CallAsync<FSNode[]>("listDir", path);
        /// <summary>
        /// Rename a file or directory.
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public Task<bool> Rename(string oldPath, string newPath) => JSRef!.CallAsync<bool>("rename", oldPath, newPath);
        ///// <summary>
        ///// Delete a file.
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //public Task<bool> DeleteFile(string path) => JSRef!.CallAsync<bool>("deleteFile", path);
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
