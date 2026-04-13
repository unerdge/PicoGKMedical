using BoxCanvasDesigner.Physics;

namespace BoxCanvasDesigner.Compliance;

/// <summary>
/// 环保评分维度
/// </summary>
public record SustainabilityDimension(
    string Name,
    float Score,
    string Description
);

/// <summary>
/// 环保评分结果
/// </summary>
public record SustainabilityReport(
    float TotalScore,
    SustainabilityDimension MaterialScore,
    SustainabilityDimension RecyclabilityScore,
    SustainabilityDimension CarbonScore,
    SustainabilityDimension EfficiencyScore
);

/// <summary>
/// 环保评分系统（文档11.2节）
/// 四维评分：材料/可回收性/碳足迹/用材效率
/// 总分0-100
/// </summary>
public class SustainabilityScorer
{
    /// <summary>
    /// 计算环保评分
    /// </summary>
    public static SustainabilityReport Calculate(
        MaterialInfo material,
        CostEstimator.PostProcessing postProcess,
        BoxParameters parameters,
        float productVolumeCM3 = 0f)
    {
        var materialDim = ScoreMaterial(material);
        var recycleDim = ScoreRecyclability(material, postProcess);
        var carbonDim = ScoreCarbon(material, parameters);
        var efficiencyDim = ScoreEfficiency(parameters, productVolumeCM3);

        float total = (materialDim.Score + recycleDim.Score + carbonDim.Score + efficiencyDim.Score) / 4f;

        return new SustainabilityReport(total, materialDim, recycleDim, carbonDim, efficiencyDim);
    }

    /// <summary>
    /// 材料评分
    /// </summary>
    private static SustainabilityDimension ScoreMaterial(MaterialInfo material)
    {
        float score = material.Type switch
        {
            MaterialType.KraftPaper => 80f,         // 牛皮纸天然环保
            MaterialType.CorrugatedE => 75f,        // 瓦楞可回收
            MaterialType.CorrugatedB => 75f,
            MaterialType.CorrugatedBC => 70f,
            MaterialType.WhiteCardboard => 60f,     // 普通原生纸张
            MaterialType.GrayCardboard => 55f,      // 含再生但不可降解
            _ => 50f
        };

        string desc = score >= 75 ? "环保材料" : score >= 60 ? "普通材料" : "建议升级环保材料";
        return new SustainabilityDimension("材料评分", score, desc);
    }

    /// <summary>
    /// 可回收性评分
    /// </summary>
    private static SustainabilityDimension ScoreRecyclability(
        MaterialInfo material, CostEstimator.PostProcessing postProcess)
    {
        float score = 80f; // 纸质基础分

        // 覆膜严重降低可回收性
        if (postProcess == CostEstimator.PostProcessing.Lamination)
        {
            score = 10f;
            return new SustainabilityDimension("可回收性", score, "覆膜后纸盒不可回收");
        }

        // UV和烫金轻微影响
        if (postProcess == CostEstimator.PostProcessing.UV)
            score -= 10f;
        if (postProcess == CostEstimator.PostProcessing.HotStamping)
            score -= 15f;

        string desc = score >= 70 ? "单一材料可回收" : "含加工层影响回收";
        return new SustainabilityDimension("可回收性", score, desc);
    }

    /// <summary>
    /// 碳足迹评分（反向计分，碳排越低分越高）
    /// </summary>
    private static SustainabilityDimension ScoreCarbon(MaterialInfo material, BoxParameters parameters)
    {
        float carbonPerKg = material.Type switch
        {
            MaterialType.CorrugatedE or MaterialType.CorrugatedB or MaterialType.CorrugatedBC => 0.75f,
            MaterialType.KraftPaper => 1.0f,
            MaterialType.WhiteCardboard => 1.2f,
            MaterialType.GrayCardboard => 1.1f,
            _ => 1.5f
        };

        // 计算包装重量
        float L = parameters.LengthMM / 1000f;
        float W = parameters.WidthMM / 1000f;
        float H = parameters.HeightMM / 1000f;
        float areaSqM = 2 * (L * W + L * H + W * H) * 1.5f;
        float weightKg = areaSqM * material.WeightGSM / 1000f;
        float carbonKg = weightKg * carbonPerKg;

        // 反向映射到0-100分
        float score = Math.Max(0f, 100f - carbonKg * 500f);

        string desc = $"单个包装碳足迹约{carbonKg * 1000:F0}g CO2";
        return new SustainabilityDimension("碳足迹", score, desc);
    }

    /// <summary>
    /// 用材效率评分
    /// </summary>
    private static SustainabilityDimension ScoreEfficiency(BoxParameters parameters, float productVolumeCM3)
    {
        if (productVolumeCM3 <= 0)
            return new SustainabilityDimension("用材效率", 70f, "未提供产品体积，使用默认评分");

        float boxVolumeCM3 = (parameters.LengthMM * parameters.WidthMM * parameters.HeightMM) / 1000f;
        float ratio = productVolumeCM3 / boxVolumeCM3;

        float score;
        string desc;

        if (ratio > 0.5f)
        {
            score = 90f;
            desc = $"用材效率优秀（体积比{ratio:F2}）";
        }
        else if (ratio > 0.3f)
        {
            score = 70f;
            desc = $"用材效率合格（体积比{ratio:F2}）";
        }
        else
        {
            score = 40f;
            desc = $"过度包装（体积比{ratio:F2}），建议缩小盒子";
        }

        return new SustainabilityDimension("用材效率", score, desc);
    }
}
