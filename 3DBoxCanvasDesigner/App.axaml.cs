using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PicoGK;
using System.Threading.Tasks;

namespace BoxCanvasDesigner;

public partial class App : Application
{
    private static bool _picoGKInitialized = false;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            // 在后台线程初始化PicoGK（只初始化一次）
            if (!_picoGKInitialized)
            {
                Task.Run(() =>
                {
                    Library.Go(0.5f, () =>
                    {
                        // PicoGK初始化完成，设置默认材质
                        Library.oViewer().SetGroupMaterial(0,
                            new ColorFloat("FF6B35"), 0.0f, 1.0f);
                        _picoGKInitialized = true;
                    });
                });
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
