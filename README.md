# NativeProxy

Proxy DLL modding with C# Native AOT.

---

C# is a good language for native modding with its native interop, but there is only two ways to run it in another executable:

- Initialize the [Common Language Runtime](https://learn.microsoft.com/en-us/dotnet/standard/clr)
  - Initializing the CLR has to be done from native code, so you will need to write a C/C++/Rust loader or use a project that contains one (like [Reloaded II](https://github.com/Reloaded-Project/Reloaded-II))
- Compile your program to a [Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot) library
  - Native AOT libraries still need to be loaded into the executable somehow, usually through DLL injection or a proxy DLL

Every option, minus a proxy DLL, requires another component to function. So let's use proxy DLLs!

This is a source generator that can take a list of DLL exports and generate magical proxy exports for it. It does this by resolving the real export at runtime and then editing the assembly of the source generated export.

## Usage

Add this to your .csproj:

```xml
<ItemGroup>
    <None Remove="NativeProxy.d3d11.txt"/>
    <AdditionalFiles Include="NativeProxy.d3d11.txt"/>
    <CompilerVisibleProperty Include="NativeProxy_OriginalPath" />
</ItemGroup>

<PropertyGroup>
    <NativeProxy_OriginalPath>C:/Windows/System32/d3d11.dll</NativeProxy_OriginalPath>
</PropertyGroup>
```

Then make an entrypoint:

```cs
public static class Proxy {
    [ProxyEntrypoint]
    public static void Main() {
        // Have fun!
    }
}
```

Note: This Main method is called from DllMain. Be careful about deadlocks - you might need another thread.

## Why modifying the function assembly?

Looking at the decompilation of the stub exports:

```text
180071c40  int64_t D3D11CreateDevice()

180071c47      int64_t pFrame = 0
180071c4c      int64_t var_10 = 0
180071c56      RhpReversePInvoke(&pFrame)
180071c56
180071c6f      for (int32_t i = 0; i s< 0x3e8; i += 1)
180071c62          S_P_CoreLib_System_Threading_Thread__Sleep(0x3e8)
180071c62
180071c81      return RhpReversePInvokeReturn(&pFrame)
```

The `RhpReversePInvoke`/`RhpReversePInvokeReturn` create problems, modifying registers and the stack when we don't want that to happen. There is no way to avoid this and just insert our own `jmp RealCode` - so we just write some assembly into memory! How fun.
