# Bootstrap

`static class`

Native-callable entry points used by the C++ dotnet_loader plugin. Every plugin assembly is loaded into its own collectible AssemblyLoadContext so plugins can carry their own dependencies.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Methods

### `void Attach(System.IntPtr gcHandle, System.IntPtr nativePlugin)`

### `int DispatchCommand(System.IntPtr gcHandle, System.IntPtr senderPtr, System.IntPtr commandNameUtf8, System.IntPtr argsUtf8, int argCount)`

Native command entry: dispatches to the managed command handler.

### `void DispatchEvent(System.IntPtr gcHandle, System.IntPtr cbHandle, System.IntPtr eventPtr)`

Native event handler entry: dispatches to the managed callback.

### `void FormDispatch(System.IntPtr playerPtr, int resultKind, ulong formId, int buttonIndex, System.IntPtr payloadUtf8)`

Native form callback entry: dispatches submit/close to the managed form.

### `int Init(System.IntPtr logFn, System.IntPtr bridgeTable)`

### `System.IntPtr LoadPlugin(System.IntPtr assemblyPathUtf8, System.IntPtr infoBuffer, int bufferSize)`

Loads a plugin assembly, finds the [Plugin]-annotated PluginBase subclass, instantiates it and returns a GCHandle. Writes JSON plugin info ({"name","version","description","authors","commands"}) — or an error message on failure — into the caller-provided UTF-8 buffer.

### `void MapRenderDispatch(System.IntPtr canvasPtr, System.IntPtr mapPtr, System.IntPtr playerPtr, ulong rendererId)`

Native map render entry: forwards endstone's render() to the managed renderer.

### `void OnDisable(System.IntPtr gcHandle)`

### `void OnEnable(System.IntPtr gcHandle)`

### `void OnLoad(System.IntPtr gcHandle)`

### `int QueryCommands(System.IntPtr gcHandle, System.IntPtr buffer, int bufferSize)`

Re-queries command declarations (called by native side after OnLoad). Writes a JSON array of command definitions; returns 1 when the buffer was written, 0 when the plugin handle is unknown.

### `void Release(System.IntPtr gcHandle)`

### `void SetServer(System.IntPtr serverPtr)`

### `void TaskDispatch(ulong managedTaskId)`

Native scheduler task entry: fires the managed callback by managed task id.

