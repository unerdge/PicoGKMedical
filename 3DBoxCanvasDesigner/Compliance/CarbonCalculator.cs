using BoxCanvasDesigner.Physics;

namespace BoxCanvasDesigner.Compliance;

/// <summary>
/// 碳足迹明细
/// </summary>
public record CarbonFootprint(
    float MaterialCarbonKg,
    float PrintingCarbonKg,
    float TransportCarbonKg,
    float TotalCarbonKg,
    float IndustryAverageKg,
    string ComparisonText,
    List<string> ReductionSuggestions
);

/// <summary>
/// 碳足迹计算器（文档11.3节）
/// 单个包装碳足迹 = 材料碳排 + 印刷碳排 + 运输碳排
/// </summary>
public class CarbonCalculator
{
    // 材料碳排系数 (kg CO2 / kg 材料)
    private static readonly Dictionary<MaterialType, float> MaterialCarbonFactor = new()
    {
        [MaterialType.WhiteCardboard] = 1.2f,
        [MaterialType.GrayCardboard] = 1.1f,
        [MaterialType.KraftPaper] = 1.0f,
        [MaterialType.CorrugatedE] = 0.75f,
        [MaterialType.CorrugatedB] = 0.80f,
        [MaterialType.CorrugatedBC] = 0.85f
    };

    // 印刷工艺碳排系数 (kg CO2 / m2)
    private static readonly Dictionary<CostEstimator.PrintingMethod, float> PrintCarbonFactor = new()
    {
        [CostEstimator.PrintingMethod.None] = 0f,
        [CostEstimator.PrintingMethod.Flexo] = 0.05f,
        [CostEstimator.PrintingMethod.Offset] = 0.08f,
        [CostEstimator.PrintingMethod.Digital] = 0.12f
    };

    // 运输碳排系数 (kg CO2 / kg / km)
    private const float TRANSPORT_FACTOR = 0.00006f; // 公路运输

    /// <summary>
    /// 计算单个包装碳足迹
    /// </summary>
    public static CarbonFootprint Calculate(
        BoxParameters parameters,
        MaterialInfo material,
        CostEstimator.PrintingMethod printing = CostEstimator.PrintingMethod.None,
        float transportDistanceKM = 500f)
    {
        // 1. 材料碳排
        float L = parameters.LengthMM / 1000f;
        float W = parameters.WidthMM / 1000f;
        float H = parameters.HeightMM / 1000f;
        float areaSqM = 2 * (L * W + L * H + W * H) * 1.5f;
        float materialWeightKg = areaSqM * material.WeightGSM / 1000f;

        float materialFactor = MaterialCarbonFactor.GetValueOrDefault(material.Type, 1.0f);
        float materialCarbon = materialWeightKg * materialFactor;

        // 2. 印刷碳排
        float printFactor = PrintCarbonFactor.GetValueOrDefault(printing, 0f);
        float printCarbon = areaSqM * printFactor;

        // 3. 运输碳排
        float transportCarbon = materialWeightKg * transportDistanceKM * TRANSPORT_FACTOR;

        // 4. 总计
        float totalCarbon = materialCarbon + printCarbon + transportCarbon;

        // 5. 行业平均值（简化估算）
        float industryAvg = 0.15f; // kg CO2/个，行业平均

        // 6. 对比
        float ratio = totalCarbon / industryAvg;
        string comparison;
        if (ratio < 0.8f) comparison = "优于行业平均水平";
        else if (ratio < 1.2f) comparison = "接近行业平均水平";
        else comparison = "高于行业平均水平";

        // 7. 减碳建议
        var suggestions = new List<string>();
        if (materialFactor > 1.0f)
            suggestions.Add($"换用再生纸/瓦楞可减少约{(materialFactor - 0.7f) / materialFactor * 100:F0}%材料碳排");
        if (printing == CostEstimator.PrintingMethod.Digital)
            suggestions.Add("数码印刷碳排较高，大批量时换用胶印可降低碳排");
        if (transportDistanceKM > 1000f)
            suggestions.Add("运输距离较远，建议就近采购以减少运输碳排");

        return new CarbonFootprint(
            materialCarbon, printCarbon, transportCarbon,
            totalCarbon, industryAvg, comparison, suggestions);
    }

    /// <summary>
    /// 计算批量碳足迹
    /// </summary>
    public static float CalculateBatch(CarbonFootprint singleFootprint, int quantity)
    {
        return singleFootprint.TotalCarbonKg * quantity;
    }
}
