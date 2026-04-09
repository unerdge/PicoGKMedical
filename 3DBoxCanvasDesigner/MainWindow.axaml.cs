using Avalonia.Controls;
using Avalonia.Interactivity;
using BoxCanvasDesigner.Dieline;
using BoxCanvasDesigner.Export;
using PicoGK;
using System;
using System.Threading.Tasks;

namespace BoxCanvasDesigner;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 从界面获取盒型参数
    /// </summary>
    private BoxParameters GetBoxParametersFromUI()
    {
        // 获取盒型类型
        BoxType boxType = BoxTypeComboBox.SelectedIndex switch
        {
            0 => BoxType.TuckEnd,
            1 => BoxType.Mailer,
            2 => BoxType.CorrugatedRSC,
            3 => BoxType.AutoLockBottom,
            4 => BoxType.PillowBox,
            5 => BoxType.RigidBox,
            _ => BoxType.TuckEnd
        };

        // 解析尺寸
        if (!float.TryParse(LengthTextBox.Text, out float length))
            length = 150f;
        if (!float.TryParse(WidthTextBox.Text, out float width))
            width = 100f;
        if (!float.TryParse(HeightTextBox.Text, out float height))
            height = 80f;
        if (!float.TryParse(WallThicknessTextBox.Text, out float wallThickness))
            wallThickness = 2.5f;

        return new BoxParameters(boxType, length, width, height, wallThickness);
    }

    /// <summary>
    /// 3D预览按钮点击
    /// </summary>
    private async void OnPreview3DClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var parameters = GetBoxParametersFromUI();
            AppendStatus($"正在生成3D预览: {parameters.Type} ({parameters.LengthMM}×{parameters.WidthMM}×{parameters.HeightMM}mm)...");

            await Task.Run(() =>
            {
                // 直接生成并预览，不再调用Library.Go()
                var generator = new BoxGenerator(parameters);
                generator.Preview();
            });

            AppendStatus("3D预览已更新");
        }
        catch (Exception ex)
        {
            AppendStatus($"错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出PDF按钮点击
    /// </summary>
    private async void OnExportPdfClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var parameters = GetBoxParametersFromUI();
            AppendStatus($"正在生成刀版: {parameters.Type}...");

            await Task.Run(() =>
            {
                var generator = new DielineGenerator();
                var dieline = generator.GenerateDieline(parameters);

                AppendStatus($"  面板数量: {dieline.Panels.Count}");
                AppendStatus($"  切割线数量: {dieline.CutLines.Count}");
                AppendStatus($"  折叠线数量: {dieline.FoldLines.Count}");
                AppendStatus($"  总面积: {dieline.TotalAreaMM2:F2} mm²");
                AppendStatus($"  边界: {dieline.Bounds.Width:F1} × {dieline.Bounds.Height:F1} mm");

                string fileName = $"Dieline_{parameters.Type}_{parameters.LengthMM}x{parameters.WidthMM}x{parameters.HeightMM}.pdf";
                var exporter = new PdfExporter();
                exporter.ExportDieline(dieline, fileName, parameters);

                AppendStatus($"✓ 已导出: {fileName}");
            });
        }
        catch (Exception ex)
        {
            AppendStatus($"错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出所有盒型按钮点击
    /// </summary>
    private async void OnExportAllClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            AppendStatus("开始导出所有盒型...\n");

            await Task.Run(() =>
            {
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
                    AppendStatus($"生成 {param.Type} 刀版...");

                    var dieline = generator.GenerateDieline(param);
                    AppendStatus($"  面板数量: {dieline.Panels.Count}");
                    AppendStatus($"  切割线数量: {dieline.CutLines.Count}");
                    AppendStatus($"  折叠线数量: {dieline.FoldLines.Count}");
                    AppendStatus($"  总面积: {dieline.TotalAreaMM2:F2} mm²");

                    string fileName = $"Dieline_{param.Type}_{param.LengthMM}x{param.WidthMM}x{param.HeightMM}.pdf";
                    exporter.ExportDieline(dieline, fileName, param);

                    AppendStatus($"  ✓ 已导出: {fileName}\n");
                }

                AppendStatus("所有刀版导出完成！");
            });
        }
        catch (Exception ex)
        {
            AppendStatus($"错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 追加状态信息
    /// </summary>
    private void AppendStatus(string message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            StatusTextBlock.Text += $"{message}\n";
        });
    }
}
