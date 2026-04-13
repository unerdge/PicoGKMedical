using BoxCanvasDesigner.Physics;

namespace BoxCanvasDesigner.Recommendation;

/// <summary>
/// 产品使用场景
/// </summary>
public enum UsageScenario
{
    EcommerceShipping,   // 电商发货
    RetailShelf,         // 零售货架
    GiftPremium,         // 送礼/高端
    FoodBeverage,        // 食品餐饮
    Industrial           // 工业/五金
}

/// <summary>
/// 用户感知脆值等级（非专业术语）
/// </summary>
public enum FragilityFeeling
{
    VeryStrong,      // 很结实
    Normal,          // 一般
    Fragile,         // 比较脆弱
    VeryFragile      // 非常脆弱
}

/// <summary>
/// 产品画像 — 用户输入的极简化信息
/// 系统在后台映射为专业参数
/// </summary>
public record ProductProfile(
    string ProductName,
    float LengthCM,
    float WidthCM,
    float HeightCM,
    float WeightKg,
    FragilityFeeling Fragility,
    UsageScenario Usage,
    int Quantity,
    float? BudgetPerUnit = null
)
{
    /// <summary>
    /// 将用户感知脆值映射为G值范围
    /// "比较脆弱" → 30G，用户永远不需要知道G值
    /// </summary>
    public float GetGFragility() => Fragility switch
    {
        FragilityFeeling.VeryStrong => 100f,
        FragilityFeeling.Normal => 60f,
        FragilityFeeling.Fragile => 30f,
        FragilityFeeling.VeryFragile => 20f,
        _ => 60f
    };

    /// <summary>
    /// 计算盒内径尺寸（产品尺寸 + 间隙）
    /// </summary>
    public (float L, float W, float H) GetBoxInnerDimensions(float clearanceMM = 10f)
    {
        return (
            LengthCM * 10f + clearanceMM,
            WidthCM * 10f + clearanceMM,
            HeightCM * 10f + clearanceMM
        );
    }

    /// <summary>
    /// 是否需要食品级材料
    /// </summary>
    public bool RequiresFoodSafe => Usage == UsageScenario.FoodBeverage;

    /// <summary>
    /// 是否为高端场景
    /// </summary>
    public bool IsPremium => Usage == UsageScenario.GiftPremium;
}
