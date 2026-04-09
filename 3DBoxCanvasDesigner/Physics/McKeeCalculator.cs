namespace BoxCanvasDesigner.Physics;

/// <summary>
/// McKee公式 - 瓦楞纸箱抗压强度计算器
/// BCT = 5.874 × ECT × CAL^0.508 × P^0.492
/// </summary>
public class McKeeCalculator
{
    /// <summary>
    /// 计算箱体抗压强度（Box Compression Test）
    /// </summary>
    /// <param name="ectLbsPerInch">边压强度 ECT (lbs/in)</param>
    /// <param name="caliperInches">瓦楞纸板厚度 (inches)</param>
    /// <param name="perimeterInches">箱体周长 = 2×(L+W) (inches)</param>
    /// <returns>抗压强度 BCT (lbs)</returns>
    public static float CalculateBCT(float ectLbsPerInch, float caliperInches, float perimeterInches)
    {
        return 5.874f * ectLbsPerInch * MathF.Pow(caliperInches, 0.508f) * MathF.Pow(perimeterInches, 0.492f);
    }

    /// <summary>
    /// 计算箱体抗压强度（使用毫米单位）
    /// </summary>
    /// <param name="parameters">盒型参数</param>
    /// <param name="material">材料信息</param>
    /// <returns>抗压强度 (kg)</returns>
    public static float CalculateBCT(BoxParameters parameters, MaterialInfo material)
    {
        // 转换单位：mm -> inches
        float lengthInches = parameters.LengthMM / 25.4f;
        float widthInches = parameters.WidthMM / 25.4f;
        float perimeterInches = 2 * (lengthInches + widthInches);
        float caliperInches = material.ThicknessMM / 25.4f;

        // 计算BCT (lbs)
        float bctLbs = CalculateBCT(material.ECT_LbsPerInch, caliperInches, perimeterInches);

        // 转换为kg
        return bctLbs * 0.453592f;
    }

    /// <summary>
    /// 计算最大堆叠层数
    /// </summary>
    /// <param name="bctKg">抗压强度 (kg)</param>
    /// <param name="boxWeightKg">单箱毛重 (kg)</param>
    /// <param name="safetyFactor">安全系数</param>
    /// <returns>最大堆叠层数</returns>
    public static int CalculateMaxStackingLayers(float bctKg, float boxWeightKg, float safetyFactor)
    {
        if (boxWeightKg <= 0) return 0;
        return (int)(bctKg / (boxWeightKg * safetyFactor));
    }

    /// <summary>
    /// 获取推荐安全系数
    /// </summary>
    public static float GetRecommendedSafetyFactor(StorageCondition condition)
    {
        return condition switch
        {
            StorageCondition.ShortTermShipping => 2.0f,      // < 10天
            StorageCondition.MediumTermStorage => 2.5f,      // 10-30天
            StorageCondition.LongTermStorage => 3.0f,        // 30-180天
            StorageCondition.HighHumidity => 4.5f,           // RH > 80%
            _ => 2.5f
        };
    }

    /// <summary>
    /// 湿度修正系数
    /// </summary>
    public static float GetHumidityCorrection(float relativeHumidity)
    {
        if (relativeHumidity < 50) return 1.0f;
        if (relativeHumidity < 70) return 0.9f;
        if (relativeHumidity < 80) return 0.7f;
        if (relativeHumidity < 90) return 0.6f;
        return 0.5f;
    }
}

/// <summary>
/// 存储条件枚举
/// </summary>
public enum StorageCondition
{
    /// <summary>短期运输 (< 10天)</summary>
    ShortTermShipping,
    /// <summary>中期仓储 (10-30天)</summary>
    MediumTermStorage,
    /// <summary>长期仓储 (30-180天)</summary>
    LongTermStorage,
    /// <summary>高湿环境 (RH > 80%)</summary>
    HighHumidity
}

/// <summary>
/// 材料信息
/// </summary>
public record MaterialInfo(
    string Name,
    MaterialType Type,
    float ThicknessMM,
    float ECT_LbsPerInch,
    float WeightGSM,
    float MaxLoadKg,
    float CostPerSqM
);

/// <summary>
/// 材料类型
/// </summary>
public enum MaterialType
{
    WhiteCardboard,      // 白卡纸 (SBS)
    GrayCardboard,       // 灰底白板纸 (CCNB)
    KraftPaper,          // 牛皮卡纸
    CorrugatedE,         // 瓦楞E楞
    CorrugatedB,         // 瓦楞B楞
    CorrugatedBC         // 瓦楞BC双层
}
