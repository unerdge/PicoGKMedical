namespace BoxCanvasDesigner.Physics;

/// <summary>
/// 成本估算引擎
/// </summary>
public class CostEstimator
{
    /// <summary>
    /// 印刷工艺类型
    /// </summary>
    public enum PrintingMethod
    {
        None,           // 无印刷
        Flexo,          // 柔印
        Offset,         // 胶印
        Digital         // 数码印刷
    }

    /// <summary>
    /// 后加工工艺
    /// </summary>
    public enum PostProcessing
    {
        None,           // 无
        Lamination,     // 覆膜
        UV,             // UV上光
        HotStamping,    // 烫金
        Embossing       // 压纹
    }

    /// <summary>
    /// 计算总成本
    /// </summary>
    public static CostBreakdown CalculateCost(
        BoxParameters parameters,
        MaterialInfo material,
        int quantity,
        PrintingMethod printing = PrintingMethod.None,
        int colorCount = 0,
        PostProcessing postProcess = PostProcessing.None)
    {
        // 1. 材料成本
        float materialCost = CalculateMaterialCost(parameters, material, quantity);

        // 2. 印刷成本
        float printingCost = CalculatePrintingCost(parameters, quantity, printing, colorCount);

        // 3. 后加工成本
        float postProcessCost = CalculatePostProcessingCost(parameters, quantity, postProcess);

        // 4. 模具分摊成本
        float dieCost = CalculateDieCost(parameters, quantity);

        // 5. 人工与管理费用（按总成本的15%估算）
        float laborCost = (materialCost + printingCost + postProcessCost) * 0.15f;

        float totalCost = materialCost + printingCost + postProcessCost + dieCost + laborCost;
        float unitCost = totalCost / quantity;

        return new CostBreakdown(
            MaterialCost: materialCost,
            PrintingCost: printingCost,
            PostProcessingCost: postProcessCost,
            DieCost: dieCost,
            LaborCost: laborCost,
            TotalCost: totalCost,
            UnitCost: unitCost,
            Quantity: quantity
        );
    }

    /// <summary>
    /// 计算材料成本
    /// </summary>
    private static float CalculateMaterialCost(BoxParameters parameters, MaterialInfo material, int quantity)
    {
        // 计算展开面积（平方米）
        float L = parameters.LengthMM / 1000f;
        float W = parameters.WidthMM / 1000f;
        float H = parameters.HeightMM / 1000f;

        // 展开面积估算：2×(L×W + L×H + W×H) × 1.5（考虑翻盖和插舌）
        float areaSqM = 2 * (L * W + L * H + W * H) * 1.5f;

        // 材料损耗率（10%）
        float wasteRate = 1.1f;

        // 总成本 = 单位面积成本 × 面积 × 数量 × 损耗率
        return material.CostPerSqM * areaSqM * quantity * wasteRate;
    }

    /// <summary>
    /// 计算印刷成本
    /// </summary>
    private static float CalculatePrintingCost(BoxParameters parameters, int quantity, PrintingMethod method, int colorCount)
    {
        if (method == PrintingMethod.None || colorCount == 0)
            return 0f;

        float areaSqM = CalculateBoxArea(parameters);

        // 印刷单价（元/平方米/色）
        float pricePerSqMPerColor = method switch
        {
            PrintingMethod.Flexo => 2.5f,
            PrintingMethod.Offset => 4.0f,
            PrintingMethod.Digital => 8.0f,
            _ => 0f
        };

        // 起版费（固定成本）
        float setupFee = method switch
        {
            PrintingMethod.Flexo => 500f * colorCount,
            PrintingMethod.Offset => 800f * colorCount,
            PrintingMethod.Digital => 0f, // 数码印刷无起版费
            _ => 0f
        };

        float printingCost = pricePerSqMPerColor * colorCount * areaSqM * quantity + setupFee;

        // 数码印刷在小批量时更经济
        if (method == PrintingMethod.Digital && quantity < 500)
            printingCost *= 0.8f;

        return printingCost;
    }

    /// <summary>
    /// 计算后加工成本
    /// </summary>
    private static float CalculatePostProcessingCost(BoxParameters parameters, int quantity, PostProcessing process)
    {
        if (process == PostProcessing.None)
            return 0f;

        float areaSqM = CalculateBoxArea(parameters);

        // 后加工单价（元/平方米）
        float pricePerSqM = process switch
        {
            PostProcessing.Lamination => 3.0f,
            PostProcessing.UV => 5.0f,
            PostProcessing.HotStamping => 15.0f,
            PostProcessing.Embossing => 12.0f,
            _ => 0f
        };

        return pricePerSqM * areaSqM * quantity;
    }

    /// <summary>
    /// 计算模具分摊成本
    /// </summary>
    private static float CalculateDieCost(BoxParameters parameters, int quantity)
    {
        // 刀模费用（固定成本，根据盒型复杂度）
        float dieBaseCost = parameters.Type switch
        {
            BoxType.TuckEnd => 800f,
            BoxType.Mailer => 600f,
            BoxType.CorrugatedRSC => 500f,
            BoxType.AutoLockBottom => 1000f,
            BoxType.PillowBox => 700f,
            BoxType.RigidBox => 1200f,
            _ => 800f
        };

        // 分摊到每个盒子
        return dieBaseCost / quantity;
    }

    /// <summary>
    /// 计算盒子面积
    /// </summary>
    private static float CalculateBoxArea(BoxParameters parameters)
    {
        float L = parameters.LengthMM / 1000f;
        float W = parameters.WidthMM / 1000f;
        float H = parameters.HeightMM / 1000f;
        return 2 * (L * W + L * H + W * H) * 1.5f;
    }

    /// <summary>
    /// 推荐最经济的印刷方案
    /// </summary>
    public static PrintingMethod RecommendPrintingMethod(int quantity, int colorCount)
    {
        if (colorCount == 0)
            return PrintingMethod.None;

        if (quantity < 500)
            return PrintingMethod.Digital; // 小批量用数码

        if (colorCount <= 2)
            return PrintingMethod.Flexo; // 单色/双色用柔印

        return PrintingMethod.Offset; // 多色用胶印
    }
}

/// <summary>
/// 成本明细
/// </summary>
public record CostBreakdown(
    float MaterialCost,
    float PrintingCost,
    float PostProcessingCost,
    float DieCost,
    float LaborCost,
    float TotalCost,
    float UnitCost,
    int Quantity
)
{
    public string GetSummary()
    {
        return $@"成本明细（数量：{Quantity}个）
材料成本：¥{MaterialCost:F2} ({MaterialCost / TotalCost * 100:F1}%)
印刷成本：¥{PrintingCost:F2} ({PrintingCost / TotalCost * 100:F1}%)
后加工成本：¥{PostProcessingCost:F2} ({PostProcessingCost / TotalCost * 100:F1}%)
模具分摊：¥{DieCost:F2} ({DieCost / TotalCost * 100:F1}%)
人工管理：¥{LaborCost:F2} ({LaborCost / TotalCost * 100:F1}%)
─────────────────────
总成本：¥{TotalCost:F2}
单价：¥{UnitCost:F2}/个";
    }
}
