namespace BoxCanvasDesigner.Physics;

/// <summary>
/// 缓冲保护算法 - G值计算与泡沫推荐
/// </summary>
public class CushionCalculator
{
    /// <summary>
    /// 产品易碎性等级
    /// </summary>
    public record FragilityLevel(
        string Name,
        float MinG,
        float MaxG,
        string TypicalProducts
    );

    private static readonly List<FragilityLevel> _fragilityLevels = new()
    {
        new("极度易碎", 15f, 25f, "精密仪器、硬盘、医疗设备"),
        new("易碎", 25f, 40f, "显示器、打印机、玻璃器皿"),
        new("中度易碎", 40f, 60f, "电视机、微波炉、小家电"),
        new("较耐冲击", 60f, 85f, "家具、五金工具"),
        new("耐冲击", 85f, 115f, "重型机械零件")
    };

    /// <summary>
    /// 泡沫材料类型
    /// </summary>
    public record FoamMaterial(
        string Name,
        float DensityKgPerM3,
        float MinThicknessMM,
        float MaxThicknessMM,
        float CostPerSqM
    );

    private static readonly List<FoamMaterial> _foamMaterials = new()
    {
        new("EPE珍珠棉", 25f, 10f, 50f, 15f),
        new("EVA泡沫", 35f, 5f, 30f, 20f),
        new("聚氨酯泡沫", 30f, 10f, 40f, 25f),
        new("气泡膜", 20f, 5f, 20f, 8f)
    };

    /// <summary>
    /// 根据重量获取推荐跌落高度
    /// </summary>
    public static float GetRecommendedDropHeight(float weightKg)
    {
        return weightKg switch
        {
            < 5f => 76f,
            < 10f => 61f,
            < 20f => 46f,
            < 45f => 30f,
            _ => 20f
        };
    }

    /// <summary>
    /// 计算所需泡沫厚度
    /// </summary>
    /// <param name="productWeightKg">产品重量 (kg)</param>
    /// <param name="gFragility">产品脆值 (G)</param>
    /// <param name="dropHeightCM">跌落高度 (cm)</param>
    /// <param name="contactAreaCM2">接触面积 (cm²)</param>
    /// <returns>推荐泡沫厚度 (mm)</returns>
    public static CushionRecommendation CalculateCushion(
        float productWeightKg,
        float gFragility,
        float dropHeightCM,
        float contactAreaCM2)
    {
        // 计算静态应力 σ = W / A (kPa)
        float staticStressKPa = (productWeightKg * 9.81f) / (contactAreaCM2 / 10000f);

        // 简化的缓冲曲线计算
        // 实际应用中需要查表或使用更复杂的模型
        float requiredThicknessMM = EstimateThickness(gFragility, dropHeightCM, staticStressKPa);

        // 推荐泡沫材料
        var recommendedFoam = SelectFoamMaterial(requiredThicknessMM, staticStressKPa);

        // 计算传递G值（简化模型）
        float transmittedG = CalculateTransmittedG(requiredThicknessMM, dropHeightCM, recommendedFoam);

        return new CushionRecommendation(
            RequiredThicknessMM: requiredThicknessMM,
            RecommendedFoam: recommendedFoam,
            StaticStressKPa: staticStressKPa,
            TransmittedG: transmittedG,
            IsSafe: transmittedG <= gFragility,
            Warnings: GenerateCushionWarnings(transmittedG, gFragility, requiredThicknessMM)
        );
    }

    /// <summary>
    /// 估算所需厚度（简化模型）
    /// </summary>
    private static float EstimateThickness(float gFragility, float dropHeightCM, float staticStressKPa)
    {
        // 简化公式：厚度与跌落高度和脆值相关
        // 实际应用需要使用缓冲曲线查表
        float baseThickness = MathF.Sqrt(dropHeightCM) * (100f / gFragility);

        // 考虑静态应力的影响
        float stressFactor = MathF.Max(1.0f, staticStressKPa / 10f);

        return MathF.Max(10f, baseThickness * stressFactor);
    }

    /// <summary>
    /// 选择合适的泡沫材料
    /// </summary>
    private static FoamMaterial SelectFoamMaterial(float requiredThicknessMM, float staticStressKPa)
    {
        foreach (var foam in _foamMaterials)
        {
            if (requiredThicknessMM >= foam.MinThicknessMM &&
                requiredThicknessMM <= foam.MaxThicknessMM)
            {
                return foam;
            }
        }
        return _foamMaterials[0]; // 默认返回EPE珍珠棉
    }

    /// <summary>
    /// 计算传递G值（简化模型）
    /// </summary>
    private static float CalculateTransmittedG(float thicknessMM, float dropHeightCM, FoamMaterial foam)
    {
        // 简化公式：传递G值与厚度成反比
        float cushionFactor = thicknessMM / 10f;
        float baseG = MathF.Sqrt(dropHeightCM) * 10f;
        return baseG / cushionFactor;
    }

    /// <summary>
    /// 生成缓冲警告
    /// </summary>
    private static List<string> GenerateCushionWarnings(float transmittedG, float gFragility, float thicknessMM)
    {
        var warnings = new List<string>();

        if (transmittedG > gFragility)
            warnings.Add($"⚠️ 警告：传递G值({transmittedG:F1}G)超过产品脆值({gFragility:F1}G)，需增加缓冲厚度");

        if (thicknessMM > 50f)
            warnings.Add($"⚠️ 注意：所需缓冲厚度较大({thicknessMM:F1}mm)，可能增加包装成本");

        if (transmittedG > gFragility * 0.9f)
            warnings.Add("⚠️ 建议：当前缓冲接近极限，建议增加10-20%安全余量");

        return warnings;
    }

    /// <summary>
    /// 获取所有易碎性等级
    /// </summary>
    public static IReadOnlyList<FragilityLevel> GetFragilityLevels() => _fragilityLevels;

    /// <summary>
    /// 获取所有泡沫材料
    /// </summary>
    public static IReadOnlyList<FoamMaterial> GetFoamMaterials() => _foamMaterials;
}

/// <summary>
/// 缓冲推荐结果
/// </summary>
public record CushionRecommendation(
    float RequiredThicknessMM,
    CushionCalculator.FoamMaterial RecommendedFoam,
    float StaticStressKPa,
    float TransmittedG,
    bool IsSafe,
    List<string> Warnings
);
