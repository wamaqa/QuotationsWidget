// Program.cs

using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using Microsoft.Windows.Widgets;
using COM;
using System;
using QuotationsWidgetProvider;
using System.Resources;


[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

[DllImport("ole32.dll")]

static extern int CoRegisterClassObject(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
            [MarshalAs(UnmanagedType.IUnknown)] object pUnk,
            uint dwClsContext,
            uint flags,
            out uint lpdwRegister);

[DllImport("ole32.dll")] static extern int CoRevokeClassObject(uint dwRegister);

Console.WriteLine("Registering Widget Provider");
uint cookie;

Guid CLSID_Factory = Guid.Parse("DFCAF762-4068-43C7-B4D5-B7CE998C1267");
CoRegisterClassObject(CLSID_Factory, new WidgetProviderFactory<WidgetProvider>(), 0x4, 0x1, out cookie);
Console.WriteLine("Registered successfully. Press ENTER to exit.");
Console.ReadLine();

if (GetConsoleWindow() != IntPtr.Zero)
{
    Console.WriteLine("Registered successfully. Press ENTER to exit.");
    Console.ReadLine();
}
else
{
    // Wait until the manager has disposed of the last widget provider.
    using (var emptyWidgetListEvent = WidgetProvider.GetEmptyWidgetListEvent())
    {
        emptyWidgetListEvent.WaitOne();
    }

    CoRevokeClassObject(cookie);
}