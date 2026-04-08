using PicoGK;
using BoxCanvasDesigner.Dieline;
using BoxCanvasDesigner.Export;

namespace BoxCanvasDesigner;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // 测试模式选择
            Console.WriteLine("=== 3D盒型画布设计器 ===");
            Console.WriteLine("1. 3D预览模式");
            Console.WriteLine("2. 刀版导出模式");
            Console.WriteLine("3. 完整测试（3D + 刀版）");
            Console.Write("请选择模式 (1-3): ");

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
                default:
                    Console.WriteLine("无效选择，运行完整测试");
                    RunFullTest();
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

            // 测试3种盒型
            var testCases = new[]
            {
                new BoxParameters(BoxType.TuckEnd, 100, 80, 120, 2.0f),
                new BoxParameters(BoxType.Mailer, 220, 150, 80, 3.0f),
                new BoxParameters(BoxType.CorrugatedRSC, 300, 200, 150, 5.0f)
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

                // 导出PDF
                string fileName = $"Dieline_{param.Type}_{param.LengthMM}x{param.WidthMM}x{param.HeightMM}.pdf";
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

