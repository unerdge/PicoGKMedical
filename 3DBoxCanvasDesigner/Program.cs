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
        RunConsoleMode();
        // if (args.Length > 0 && args[0] == "--console")
        // {
        //     RunConsoleMode();
        // }
        // else
        // {
        //     // 启动GUI模式
        //     BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        // }
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
        Library.Go(0.5f, Example.Run3DPreview);
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

        // 再显示3D预览
        Console.WriteLine("\n按任意键继续查看3D预览...");
        Console.ReadKey();
        Library.Go(0.5f, Example.Run3DPreview);
    }
}

class Example
{
    /// <summary>
    /// 3D预览
    /// </summary>
    public static void Run3DPreview()
    {
        try
        {
            Library.oViewer().SetGroupMaterial(0, "FF6B35", 0.0f, 1.0f); // 橙色

            // 创建盒型参数 - 邮寄盒示例
            var parameters = new BoxParameters(
                Type: BoxType.Mailer,
                LengthMM: 220,
                WidthMM: 150,
                HeightMM: 80,
                WallThicknessMM: 3.0f
            );

            // 生成并预览盒子
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
