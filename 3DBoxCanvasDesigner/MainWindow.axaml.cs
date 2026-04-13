using Avalonia.Controls;
using Avalonia.Interactivity;
using BoxCanvasDesigner.Compliance;
using BoxCanvasDesigner.Dieline;
using BoxCanvasDesigner.Export;
using BoxCanvasDesigner.Physics;
using BoxCanvasDesigner.Printing;
using BoxCanvasDesigner.Prototyping;
using BoxCanvasDesigner.Recommendation;
using BoxCanvasDesigner.Validation;
using PicoGK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxCanvasDesigner;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // ─────────── 通用工具方法 ───────────

    private BoxParameters GetBoxParametersFromUI()
    {
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

        if (!float.TryParse(LengthTextBox.Text, out float length)) length = 150f;
        if (!float.TryParse(WidthTextBox.Text, out float width)) width = 100f;
        if (!float.TryParse(HeightTextBox.Text, out float height)) height = 80f;
        if (!float.TryParse(WallThicknessTextBox.Text, out float wallThickness)) wallThickness = 2.5f;

        return new BoxParameters(boxType, length, width, height, wallThickness);
    }

    private void SetText(TextBlock block, string text)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => block.Text = text);
    }

    private void AppendText(TextBlock block, string text)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => block.Text += text + "\n");
    }

    // ─────────── Tab1: 智能推荐 ───────────

    private async void OnRecommendClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            SetText(RecommendResultText, "正在求解最优方案...");

            if (!float.TryParse(ProdLengthBox.Text, out float pL)) pL = 16f;
            if (!float.TryParse(ProdWidthBox.Text, out float pW)) pW = 8f;
            if (!float.TryParse(ProdHeightBox.Text, out float pH)) pH = 1.5f;
            if (!float.TryParse(ProdWeightBox.Text, out float pWeight)) pWeight = 0.05f;
            if (!int.TryParse(QuantityBox.Text, out int qty)) qty = 500;
            float.TryParse(BudgetBox.Text, out float budget);

            var fragility = FragilityCombo.SelectedIndex switch
            {
                0 => FragilityFeeling.VeryStrong,
                1 => FragilityFeeling.Normal,
                2 => FragilityFeeling.Fragile,
                3 => FragilityFeeling.VeryFragile,
                _ => FragilityFeeling.Normal
            };

            var usage = UsageCombo.SelectedIndex switch
            {
                0 => UsageScenario.EcommerceShipping,
                1 => UsageScenario.RetailShelf,
                2 => UsageScenario.GiftPremium,
                3 => UsageScenario.FoodBeverage,
                4 => UsageScenario.Industrial,
                _ => UsageScenario.EcommerceShipping
            };

            var profile = new ProductProfile(
                ProductNameBox.Text ?? "产品",
                pL, pW, pH, pWeight,
                fragility, usage, qty,
                budget > 0 ? budget : null);

            string result = await Task.Run(() =>
            {
                var solver = new PackagingSolver();
                var recommendation = solver.Solve(profile);
                return recommendation.GetFullReport();
            });

            SetText(RecommendResultText, result);
        }
        catch (Exception ex)
        {
            SetText(RecommendResultText, $"错误: {ex.Message}");
        }
    }

    // ─────────── Tab2: 盒型设计 ───────────

    private async void OnPreview3DClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var parameters = GetBoxParametersFromUI();
            SetText(DesignStatusText, $"正在生成3D预览: {parameters.Type}...");

            await Task.Run(() =>
            {
                var generator = new BoxGenerator(parameters);
                generator.Preview();
            });

            AppendText(DesignStatusText, "3D预览已更新");
        }
        catch (Exception ex)
        {
            SetText(DesignStatusText, $"错误: {ex.Message}");
        }
    }

    private void OnValidateClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var parameters = GetBoxParametersFromUI();
            var validator = new StructureValidator();
            var result = validator.Validate(parameters);

            string text = result.IsValid ? "结构验证通过\n" : "结构验证未通过\n";

            foreach (var err in result.Errors)
                text += $"  [错误] {err}\n";
            foreach (var warn in result.Warnings)
                text += $"  [警告] {warn}\n";

            SetText(DesignStatusText, text);
        }
        catch (Exception ex)
        {
            SetText(DesignStatusText, $"错误: {ex.Message}");
        }
    }

    // ─────────── Tab3: 材料与成本 ───────────

    private void OnCalculateCostClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var parameters = GetBoxParametersFromUI();
            if (!float.TryParse(CostWeightBox.Text, out float weight)) weight = 2.5f;
            if (!int.TryParse(CostQuantityBox.Text, out int quantity)) quantity = 1000;

            var printing = PrintMethodCombo.SelectedIndex switch
            {
                0 => CostEstimator.PrintingMethod.None,
                1 => CostEstimator.PrintingMethod.Flexo,
                2 => CostEstimator.PrintingMethod.Offset,
                3 => CostEstimator.PrintingMethod.Digital,
                _ => CostEstimator.PrintingMethod.None
            };
            int colorCount = printing == CostEstimator.PrintingMethod.None ? 0 : 4;

            // 推荐材料
            var material = MaterialLibrary.GetRecommendedMaterial(weight, parameters);
            if (material == null)
            {
                SetText(CostResultText, "找不到合适的材料");
                return;
            }

            var cost = CostEstimator.CalculateCost(parameters, material, quantity, printing, colorCount);

            string text = $"推荐材料: {material.Name}\n";
            text += $"最大承重: {material.MaxLoadKg}kg\n\n";
            text += cost.GetSummary();

            SetText(CostResultText, text);
        }
        catch (Exception ex)
        {
            SetText(CostResultText, $"错误: {ex.Message}");
        }
    }

    // ─────────── Tab4: 物理验证 ───────────

    private void OnPhysicsCheckClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var parameters = GetBoxParametersFromUI();
            if (!float.TryParse(CostWeightBox.Text, out float weight)) weight = 2.5f;

            var material = MaterialLibrary.GetRecommendedMaterial(weight, parameters);
            if (material == null)
            {
                SetText(PhysicsResultText, "找不到合适的材料");
                return;
            }

            var stacking = StackingAdvisor.CalculateStacking(
                parameters, material, weight, StorageCondition.MediumTermStorage);

            var mullen = MullenTable.GetRecommendedGrade(weight);

            string text = "=== 结构力学验证 ===\n";
            text += $"材料: {material.Name}\n";
            text += $"BCT抗压强度: {stacking.BCT_Kg:F1} kg\n";
            text += $"最大堆叠层数: {stacking.MaxStackingLayers} 层\n";
            text += $"安全系数: {stacking.SafetyFactor}x\n\n";

            text += "=== Mullen耐破 ===\n";
            text += $"推荐等级: {mullen.Name} ({mullen.MullenPSI} psi)\n";
            text += $"最大装载: {mullen.MaxLoadKg} kg\n\n";

            text += "=== 缓冲保护 ===\n";
            float dropH = CushionCalculator.GetRecommendedDropHeight(weight);
            text += $"推荐跌落高度: {dropH} cm\n";

            foreach (var warn in stacking.Warnings)
                text += $"  {warn}\n";

            SetText(PhysicsResultText, text);
        }
        catch (Exception ex)
        {
            SetText(PhysicsResultText, $"错误: {ex.Message}");
        }
    }

    // ─────────── Tab5: 导出中心 ───────────

    private async void OnExportPdfClick(object? sender, RoutedEventArgs e)
    {
        await ExportDieline("PDF", (dieline, fileName, parameters) =>
        {
            new PdfExporter().ExportDieline(dieline, fileName + ".pdf", parameters);
        });
    }

    private async void OnExportSvgClick(object? sender, RoutedEventArgs e)
    {
        await ExportDieline("SVG", (dieline, fileName, parameters) =>
        {
            new SvgExporter().ExportDieline(dieline, fileName + ".svg", parameters);
        });
    }

    private async void OnExportDxfClick(object? sender, RoutedEventArgs e)
    {
        await ExportDieline("DXF", (dieline, fileName, parameters) =>
        {
            new DxfExporter().ExportDieline(dieline, fileName + ".dxf", parameters);
        });
    }

    private async void OnExportStlClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var parameters = GetBoxParametersFromUI();
            SetText(ExportStatusText, "正在导出STL...");

            await Task.Run(() =>
            {
                var generator = new BoxGenerator(parameters);
                var voxels = generator.GenerateVoxels();
                string fileName = $"Box_{parameters.Type}_{parameters.LengthMM}x{parameters.WidthMM}x{parameters.HeightMM}.stl";
                new StlExporter().ExportBox(voxels, fileName, parameters);
                AppendText(ExportStatusText, $"已导出: {fileName}");
            });
        }
        catch (Exception ex)
        {
            SetText(ExportStatusText, $"错误: {ex.Message}");
        }
    }

    private async Task ExportDieline(string format, Action<DielineData, string, BoxParameters> exportAction)
    {
        try
        {
            var parameters = GetBoxParametersFromUI();
            SetText(ExportStatusText, $"正在导出{format}...");

            await Task.Run(() =>
            {
                var dieline = new DielineGenerator().GenerateDieline(parameters);
                string fileName = $"Dieline_{parameters.Type}_{parameters.LengthMM}x{parameters.WidthMM}x{parameters.HeightMM}";
                exportAction(dieline, fileName, parameters);
                AppendText(ExportStatusText, $"已导出: {fileName}.{format.ToLower()}");
            });
        }
        catch (Exception ex)
        {
            SetText(ExportStatusText, $"错误: {ex.Message}");
        }
    }

    private async void OnExportAllClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            SetText(ExportStatusText, "开始导出所有盒型PDF...\n");

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
                    var dieline = generator.GenerateDieline(param);
                    string fileName = $"Dieline_{param.Type}_{param.LengthMM}x{param.WidthMM}x{param.HeightMM}.pdf";
                    exporter.ExportDieline(dieline, fileName, param);
                    AppendText(ExportStatusText, $"已导出: {fileName}");
                }

                AppendText(ExportStatusText, "\n所有盒型导出完成!");
            });
        }
        catch (Exception ex)
        {
            AppendText(ExportStatusText, $"错误: {ex.Message}");
        }
    }

    private void OnCheck3DPrintClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var parameters = GetBoxParametersFromUI();
            var tech = Print3DTechCombo.SelectedIndex switch
            {
                0 => PrintTech3D.FDM,
                1 => PrintTech3D.SLA,
                2 => PrintTech3D.SLS,
                3 => PrintTech3D.MJF,
                _ => PrintTech3D.FDM
            };

            float scaledWall = Print3DExporter.ScaleWallThickness(parameters.WallThicknessMM, tech);
            var (fits, printer, suggestion) = Print3DExporter.CheckPrintBedFit(parameters);

            string text = $"=== 3D打印适配检查 ({tech}) ===\n";
            text += $"原始壁厚: {parameters.WallThicknessMM}mm → 打印壁厚: {scaledWall}mm\n";
            text += fits
                ? $"打印平台: {printer?.PrinterName} 可容纳\n"
                : $"警告: {suggestion}\n";

            // 打样路径推荐
            var material = MaterialLibrary.GetAllMaterials()[0];
            var proto = PrototypingAdvisor.Recommend(parameters, material);
            text += $"\n=== 打样推荐 ===\n";
            text += $"路径: {proto.PathDescription}\n";
            text += $"材料: {proto.RecommendedMaterial}\n";
            text += $"时间: {proto.EstimatedTime}\n";
            text += $"成本: {proto.EstimatedCost}\n";

            SetText(ExportStatusText, text);
        }
        catch (Exception ex)
        {
            SetText(ExportStatusText, $"错误: {ex.Message}");
        }
    }

    // ─────────── Tab6: 环保评分 ───────────

    private void OnSustainabilityClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var parameters = GetBoxParametersFromUI();
            if (!float.TryParse(CostWeightBox.Text, out float weight)) weight = 2.5f;

            var material = MaterialLibrary.GetRecommendedMaterial(weight, parameters);
            if (material == null)
            {
                SetText(SustainabilityText, "找不到合适的材料");
                return;
            }

            // 环保评分
            var sustainability = SustainabilityScorer.Calculate(
                material, CostEstimator.PostProcessing.None, parameters);

            // 碳足迹
            var carbon = CarbonCalculator.Calculate(parameters, material);

            // 合规检查
            var compliance = ComplianceChecker.CheckAll(parameters, material, false, false, false);

            string text = $"=== 环保评分: {sustainability.TotalScore:F0}/100 ===\n";
            text += $"  {sustainability.MaterialScore.Name}: {sustainability.MaterialScore.Score:F0} - {sustainability.MaterialScore.Description}\n";
            text += $"  {sustainability.RecyclabilityScore.Name}: {sustainability.RecyclabilityScore.Score:F0} - {sustainability.RecyclabilityScore.Description}\n";
            text += $"  {sustainability.CarbonScore.Name}: {sustainability.CarbonScore.Score:F0} - {sustainability.CarbonScore.Description}\n";
            text += $"  {sustainability.EfficiencyScore.Name}: {sustainability.EfficiencyScore.Score:F0} - {sustainability.EfficiencyScore.Description}\n\n";

            text += $"=== 碳足迹 ===\n";
            text += $"  材料碳排: {carbon.MaterialCarbonKg * 1000:F1}g CO2\n";
            text += $"  印刷碳排: {carbon.PrintingCarbonKg * 1000:F1}g CO2\n";
            text += $"  运输碳排: {carbon.TransportCarbonKg * 1000:F1}g CO2\n";
            text += $"  总计: {carbon.TotalCarbonKg * 1000:F1}g CO2/个\n";
            text += $"  {carbon.ComparisonText}\n\n";

            text += "=== 合规检查 ===\n";
            foreach (var item in compliance)
            {
                string icon = item.IsCompliant ? "OK" : "!!";
                text += $"  [{icon}] {item.Type}: {item.Description}\n";
                if (item.Suggestion != null)
                    text += $"       建议: {item.Suggestion}\n";
            }

            SetText(SustainabilityText, text);
        }
        catch (Exception ex)
        {
            SetText(SustainabilityText, $"错误: {ex.Message}");
        }
    }
}
