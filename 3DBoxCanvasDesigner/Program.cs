using Avalonia;
using PicoGK;
using BoxCanvasDesigner.Dieline;
using BoxCanvasDesigner.Export;
using BoxCanvasDesigner.Examples;

namespace BoxCanvasDesigner;

class Program
{
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    static void Main(string[] args)
    {
        // 如果有命令行参数，使用旧的控制台模式
        // RunConsoleMode();
        if (args.Length > 0 && args[0] == "--console")
        {
            RunConsoleMode();
        }
        else
        {
            // 启动GUI模式
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
    }

    /// <summary>
    /// 控制台模式（保留原有功能）
    /// </summary>
    static void RunConsoleMode()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
        try
        {
            // 测试模式选择
            Console.WriteLine("=== 3D盒型画布设计器 ===");
            Console.WriteLine("1. 3D预览模式");
            Console.WriteLine("2. 刀版导出模式");
            Console.WriteLine("3. 完整测试（3D + 刀版）");
            Console.WriteLine("4. 物理引擎演示");
            Console.Write("请选择模式 (1-4): ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Run3DPreview();
                    break;
                case "2":
                    RunDielineExport();
                    break;
                case "3":
                    RunFullTest();
                    break;
                case "4":
                    PhysicsEngineDemo.RunDemo();
                    break;
                default:
                    Console.WriteLine("无效选择，运行物理引擎演示");
                    PhysicsEngineDemo.RunDemo();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    /// <summary>
    /// 3D预览模式
    /// </summary>
    static void Run3DPreview()
    {
        Console.WriteLine("\n=== 3D预览模式 ===");
        Console.WriteLine("选择盒型:");
        Console.WriteLine("  1. 插舌式盒 (TuckEnd)           100 x  80 x 120 mm  壁厚 2.0 mm");
        Console.WriteLine("  2. 邮寄盒 (Mailer)              220 x 150 x  80 mm  壁厚 3.0 mm");
        Console.WriteLine("  3. 瓦楞标准开槽箱 (RSC)         300 x 200 x 150 mm  壁厚 5.0 mm");
        Console.WriteLine("  4. 自动锁底盒 (AutoLockBottom)  150 x 100 x 100 mm  壁厚 2.5 mm");
        Console.WriteLine("  5. 枕头盒 (PillowBox)           120 x  80 x  60 mm  壁厚 2.0 mm");
        Console.WriteLine("  6. 天地盖精装盒 (RigidBox)      200 x 150 x  80 mm  壁厚 3.0 mm");
        Console.Write("请选择 (1-6, 默认2): ");

        BoxParameters parameters = Console.ReadLine()?.Trim() switch
        {
            "1" => new BoxParameters(BoxType.TuckEnd,        100,  80, 120, 2.0f),
            "3" => new BoxParameters(BoxType.CorrugatedRSC,  300, 200, 150, 5.0f),
            "4" => new BoxParameters(BoxType.AutoLockBottom, 150, 100, 100, 2.5f),
            "5" => new BoxParameters(BoxType.PillowBox,      120,  80,  60, 2.0f),
            "6" => new BoxParameters(BoxType.RigidBox,       200, 150,  80, 3.0f),
            _   => new BoxParameters(BoxType.Mailer,         220, 150,  80, 3.0f)
        };

        Library.Go(0.5f, () => Example.Run3DPreview(parameters));
    }

    /// <summary>
    /// 刀版导出模式
    /// </summary>
    static void RunDielineExport()
    {
        Example.RunDielineExport();
    }

    /// <summary>
    /// 完整测试
    /// </summary>
    static void RunFullTest()
    {
        // 先导出刀版
        Example.RunDielineExport();

        // 再选择盒型显示3D预览
        Console.WriteLine("\n按任意键继续查看3D预览...");
        Console.ReadKey();
        Run3DPreview();
    }
}

class Example
{
    /// <summary>
    /// 3D预览
    /// </summary>
    public static void Run3DPreview(BoxParameters parameters)
    {
        try
        {
            Library.oViewer().SetGroupMaterial(0, new PicoGK.ColorFloat("FF6B35FF"), 0.0f, 0.8f);

            var generator = new BoxGenerator(parameters);
            generator.Preview();

            Console.WriteLine("3D预览已加载 - 可以旋转查看");
        }
        catch (Exception ex)
        {
            Library.Log($"运行错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 刀版导出
    /// </summary>
    public static void RunDielineExport()
    {
        try
        {
            Console.WriteLine("\n=== 刀版导出测试 ===\n");

            // 测试6种盒型
            var testCases = new[]
            {
                new BoxParameters(BoxType.TuckEnd, 100, 80, 120, 2.0f),
                new BoxParameters(BoxType.Mailer, 220, 150, 80, 3.0f),
                new BoxParameters(BoxType.CorrugatedRSC, 300, 200, 150, 5.0f),
                new BoxParameters(BoxType.AutoLockBottom, 150, 100, 100, 2.5f),
                new BoxParameters(BoxType.PillowBox, 120, 80, 60, 2.0f),
                new BoxParameters(BoxType.RigidBox, 200, 150, 80, 3.0f)
            };

            var generator = new DielineGenerator();
            var exporter = new PdfExporter();

            foreach (var param in testCases)
            {
                Console.WriteLine($"生成 {param.Type} 刀版...");

                // 生成刀版数据
                var dieline = generator.GenerateDieline(param);

                // 显示统计信息
                Console.WriteLine($"  面板数量: {dieline.Panels.Count}");
                Console.WriteLine($"  切割线数量: {dieline.CutLines.Count}");
                Console.WriteLine($"  折叠线数量: {dieline.FoldLines.Count}");
                Console.WriteLine($"  总面积: {dieline.TotalAreaMM2:F2} mm²");
                Console.WriteLine($"  边界: {dieline.Bounds.Width:F1} × {dieline.Bounds.Height:F1} mm");

                // 导出PDF（输出到项目目录下的 log/PDF/）
                // AppContext.BaseDirectory = bin/Debug/net10.0/，上移三级即项目根目录
                string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
                string outputDir = Path.Combine(projectDir, "log", "PDF");
                Directory.CreateDirectory(outputDir);
                string fileName = Path.Combine(outputDir, $"Dieline_{param.Type}_{param.LengthMM}x{param.WidthMM}x{param.HeightMM}.pdf");
                exporter.ExportDieline(dieline, fileName, param);

                Console.WriteLine($"  ✓ 已导出: {fileName}\n");
            }

            Console.WriteLine("所有刀版导出完成！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"导出错误: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
