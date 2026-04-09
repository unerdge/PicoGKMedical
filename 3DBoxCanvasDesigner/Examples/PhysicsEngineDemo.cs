using BoxCanvasDesigner;
using BoxCanvasDesigner.Physics;

namespace BoxCanvasDesigner.Examples;

/// <summary>
/// 物理引擎测试示例
/// </summary>
public class PhysicsEngineDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("=== 3D盒型画布设计器 - 物理引擎演示 ===\n");

        // 示例1：电商快递盒设计
        Console.WriteLine("【示例1：电商快递盒设计】");
        DemoEcommerceBox();

        Console.WriteLine("\n" + new string('=', 60) + "\n");

        // 示例2：精密仪器包装设计
        Console.WriteLine("【示例2：精密仪器包装设计】");
        DemoPrecisionInstrumentBox();

        Console.WriteLine("\n" + new string('=', 60) + "\n");

        // 示例3：成本优化对比
        Console.WriteLine("【示例3：成本优化对比】");
        DemoCostComparison();
    }

    /// <summary>
    /// 示例1：电商快递盒
    /// </summary>
    private static void DemoEcommerceBox()
    {
        // 盒型参数
        var parameters = new BoxParameters(
            Type: BoxType.Mailer,
            LengthMM: 300f,
            WidthMM: 200f,
            HeightMM: 150f,
            WallThicknessMM: 3.0f
        );

        // 产品信息
        float productWeightKg = 2.5f;
        int quantity = 1000;

        Console.WriteLine($"盒型：{parameters.Type}");
        Console.WriteLine($"尺寸：{parameters.LengthMM}×{parameters.WidthMM}×{parameters.HeightMM}mm");
        Console.WriteLine($"产品重量：{productWeightKg}kg");
        Console.WriteLine($"订单数量：{quantity}个\n");

        // 1. 材料推荐
        var recommendedMaterial = MaterialLibrary.GetRecommendedMaterial(productWeightKg, parameters);
        Console.WriteLine($"推荐材料：{recommendedMaterial?.Name}");
        Console.WriteLine($"  厚度：{recommendedMaterial?.ThicknessMM}mm");
        Console.WriteLine($"  最大承重：{recommendedMaterial?.MaxLoadKg}kg");
        Console.WriteLine($"  单价：¥{recommendedMaterial?.CostPerSqM}/m²\n");

        // 2. 结构强度验证
        if (recommendedMaterial != null)
        {
            var stackingResult = StackingAdvisor.CalculateStacking(
                parameters,
                recommendedMaterial,
                productWeightKg,
                StorageCondition.MediumTermStorage,
                relativeHumidity: 60f
            );

            Console.WriteLine("结构强度分析：");
            Console.WriteLine($"  理论抗压强度(BCT)：{stackingResult.BCT_Kg:F1}kg");
            Console.WriteLine($"  修正后强度：{stackingResult.CorrectedBCT_Kg:F1}kg");
            Console.WriteLine($"  安全系数：{stackingResult.SafetyFactor}×");
            Console.WriteLine($"  最大堆叠层数：{stackingResult.MaxStackingLayers}层");
            Console.WriteLine($"  单箱毛重：{stackingResult.BoxWeightKg:F2}kg");

            if (stackingResult.Warnings.Count > 0)
            {
                Console.WriteLine("  警告：");
                foreach (var warning in stackingResult.Warnings)
                    Console.WriteLine($"    {warning}");
            }
            Console.WriteLine();

            // 3. 成本估算
            var cost = CostEstimator.CalculateCost(
                parameters,
                recommendedMaterial,
                quantity,
                CostEstimator.PrintingMethod.Flexo,
                colorCount: 2
            );

            Console.WriteLine(cost.GetSummary());
        }
    }

    /// <summary>
    /// 示例2：精密仪器包装
    /// </summary>
    private static void DemoPrecisionInstrumentBox()
    {
        var parameters = new BoxParameters(
            Type: BoxType.RigidBox,
            LengthMM: 250f,
            WidthMM: 180f,
            HeightMM: 120f,
            WallThicknessMM: 3.0f
        );

        float productWeightKg = 1.2f;
        float gFragility = 20f; // 极度易碎

        Console.WriteLine($"盒型：{parameters.Type}");
        Console.WriteLine($"产品：精密仪器（{productWeightKg}kg，脆值{gFragility}G）\n");

        // 缓冲保护计算
        float dropHeight = CushionCalculator.GetRecommendedDropHeight(productWeightKg);
        float contactArea = (parameters.LengthMM * parameters.WidthMM) * 0.8f / 100f; // 80%接触面积，转cm²

        var cushionResult = CushionCalculator.CalculateCushion(
            productWeightKg,
            gFragility,
            dropHeight,
            contactArea
        );

        Console.WriteLine("缓冲保护分析：");
        Console.WriteLine($"  推荐跌落高度：{dropHeight}cm");
        Console.WriteLine($"  所需泡沫厚度：{cushionResult.RequiredThicknessMM:F1}mm");
        Console.WriteLine($"  推荐泡沫材料：{cushionResult.RecommendedFoam.Name}");
        Console.WriteLine($"  泡沫密度：{cushionResult.RecommendedFoam.DensityKgPerM3}kg/m³");
        Console.WriteLine($"  静态应力：{cushionResult.StaticStressKPa:F2}kPa");
        Console.WriteLine($"  传递G值：{cushionResult.TransmittedG:F1}G");
        Console.WriteLine($"  安全性：{(cushionResult.IsSafe ? "✓ 安全" : "✗ 不安全")}");

        if (cushionResult.Warnings.Count > 0)
        {
            Console.WriteLine("  警告：");
            foreach (var warning in cushionResult.Warnings)
                Console.WriteLine($"    {warning}");
        }
    }

    /// <summary>
    /// 示例3：成本优化对比
    /// </summary>
    private static void DemoCostComparison()
    {
        var parameters = new BoxParameters(
            Type: BoxType.TuckEnd,
            LengthMM: 150f,
            WidthMM: 100f,
            HeightMM: 80f,
            WallThicknessMM: 2.5f
        );

        int quantity = 500;

        Console.WriteLine($"盒型：{parameters.Type}");
        Console.WriteLine($"数量：{quantity}个\n");

        // 方案A：最经济（单色柔印）
        var materialA = MaterialLibrary.GetMaterialByName("白卡纸300gsm");
        var costA = CostEstimator.CalculateCost(
            parameters, materialA!, quantity,
            CostEstimator.PrintingMethod.Flexo, 1
        );

        Console.WriteLine("【方案A：最经济】");
        Console.WriteLine($"材料：{materialA?.Name}");
        Console.WriteLine($"印刷：单色柔印");
        Console.WriteLine($"单价：¥{costA.UnitCost:F2}/个");
        Console.WriteLine($"总成本：¥{costA.TotalCost:F2}\n");

        // 方案B：推荐（四色数码）
        var materialB = MaterialLibrary.GetMaterialByName("白卡纸350gsm");
        var costB = CostEstimator.CalculateCost(
            parameters, materialB!, quantity,
            CostEstimator.PrintingMethod.Digital, 4
        );

        Console.WriteLine("【方案B：推荐（性价比最优）】");
        Console.WriteLine($"材料：{materialB?.Name}");
        Console.WriteLine($"印刷：四色数码");
        Console.WriteLine($"单价：¥{costB.UnitCost:F2}/个");
        Console.WriteLine($"总成本：¥{costB.TotalCost:F2}\n");

        // 方案C：高端（覆膜+烫金）
        var materialC = MaterialLibrary.GetMaterialByName("白卡纸400gsm");
        var costC = CostEstimator.CalculateCost(
            parameters, materialC!, quantity,
            CostEstimator.PrintingMethod.Offset, 4,
            CostEstimator.PostProcessing.HotStamping
        );

        Console.WriteLine("【方案C：高端体验】");
        Console.WriteLine($"材料：{materialC?.Name}");
        Console.WriteLine($"印刷：四色胶印+烫金");
        Console.WriteLine($"单价：¥{costC.UnitCost:F2}/个");
        Console.WriteLine($"总成本：¥{costC.TotalCost:F2}\n");

        Console.WriteLine($"成本差异：方案B比方案A贵{(costB.UnitCost - costA.UnitCost) / costA.UnitCost * 100:F1}%");
        Console.WriteLine($"成本差异：方案C比方案B贵{(costC.UnitCost - costB.UnitCost) / costB.UnitCost * 100:F1}%");
    }
}
