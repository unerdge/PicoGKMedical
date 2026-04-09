namespace BoxCanvasDesigner.Physics;

/// <summary>
/// 堆叠安全顾问
/// </summary>
public class StackingAdvisor
{
    /// <summary>
    /// 计算堆叠建议
    /// </summary>
    public static StackingRecommendation CalculateStacking(
        BoxParameters parameters,
        MaterialInfo material,
        float productWeightKg,
        StorageCondition condition,
        float relativeHumidity = 50f)
    {
        // 计算BCT抗压强度
        float bctKg = McKeeCalculator.CalculateBCT(parameters, material);

        // 应用湿度修正
        float humidityCorrection = McKeeCalculator.GetHumidityCorrection(relativeHumidity);
        float correctedBCT = bctKg * humidityCorrection;

        // 获取安全系数
        float safetyFactor = McKeeCalculator.GetRecommendedSafetyFactor(condition);

        // 计算单箱毛重（产品重量 + 包装重量）
        float boxWeightKg = productWeightKg + EstimateBoxWeight(parameters, material);

        // 计算最大堆叠层数
        int maxLayers = McKeeCalculator.CalculateMaxStackingLayers(correctedBCT, boxWeightKg, safetyFactor);

        // 生成建议
        return new StackingRecommendation(
            BCT_Kg: bctKg,
            CorrectedBCT_Kg: correctedBCT,
            SafetyFactor: safetyFactor,
            MaxStackingLayers: maxLayers,
            BoxWeightKg: boxWeightKg,
            IsSafe: maxLayers >= 1,
            Warnings: GenerateWarnings(maxLayers, correctedBCT, bctKg, relativeHumidity)
        );
    }

    /// <summary>
    /// 估算盒子自重
    /// </summary>
    private static float EstimateBoxWeight(BoxParameters parameters, MaterialInfo material)
    {
        // 计算盒子表面积（展开面积）
        float L = parameters.LengthMM / 1000f; // 转换为米
        float W = parameters.WidthMM / 1000f;
        float H = parameters.HeightMM / 1000f;

        // 简化估算：2×(L×W + L×H + W×H) + 翻盖面积
        float areaSqM = 2 * (L * W + L * H + W * H) * 1.5f; // 1.5倍系数考虑翻盖和插舌

        // 重量 = 面积 × 克重 / 1000
        return areaSqM * material.WeightGSM / 1000f;
    }

    /// <summary>
    /// 生成警告信息
    /// </summary>
    private static List<string> GenerateWarnings(int maxLayers, float correctedBCT, float originalBCT, float humidity)
    {
        var warnings = new List<string>();

        if (maxLayers < 1)
            warnings.Add("⚠️ 警告：当前设计无法承受堆叠，建议升级材料或减小尺寸");
        else if (maxLayers < 3)
            warnings.Add("⚠️ 注意：堆叠能力较弱，仅支持少量堆叠");

        if (humidity > 80)
            warnings.Add($"⚠️ 高湿环境：强度下降{(1 - correctedBCT / originalBCT) * 100:F0}%");

        return warnings;
    }
}

/// <summary>
/// 堆叠建议结果
/// </summary>
public record StackingRecommendation(
    float BCT_Kg,
    float CorrectedBCT_Kg,
    float SafetyFactor,
    int MaxStackingLayers,
    float BoxWeightKg,
    bool IsSafe,
    List<string> Warnings
);
