using System.Windows;

namespace Laboratorium4.Zadanie1.NET;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _ = AsyncServer.StartServerAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        AsyncServer.StopServer();
    }
}