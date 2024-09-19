using System.Runtime.InteropServices;

namespace NativeProxy.Sample;

public static class Proxy {
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int MessageBoxW(nint hwnd, string text, string caption, uint type);

    [ProxyEntrypoint]
    public static void Main() {
        _ = MessageBoxW(nint.Zero, "Hello, world!", "NativeProxy.Sample", 0);
    }
}
