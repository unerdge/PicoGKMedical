namespace BoxCanvasDesigner;

/// <summary>
/// 盒型类型枚举
/// </summary>
public enum BoxType
{
    /// <summary>插舌式盒（Tuck End Box）</summary>
    TuckEnd,

    /// <summary>邮寄盒（Mailer Box）</summary>
    Mailer,

    /// <summary>瓦楞标准开槽箱（Regular Slotted Container）</summary>
    CorrugatedRSC,

    /// <summary>自动锁底盒（Auto Lock Bottom Box）</summary>
    AutoLockBottom,

    /// <summary>枕头盒（Pillow Box）</summary>
    PillowBox,

    /// <summary>天地盖精装盒（Rigid Box / Lid and Base）</summary>
    RigidBox
}

/// <summary>
/// 锁扣类型枚举
/// </summary>
public enum LockType
{
    /// <summary>无锁扣</summary>
    None,
    /// <summary>插舌锁扣</summary>
    Tuck,
    /// <summary>自动锁底</summary>
    AutoLock,
    /// <summary>胶水粘合</summary>
    Glue
}

/// <summary>
/// 盒型参数
/// </summary>
/// <param name="Type">盒型类型</param>
/// <param name="LengthMM">内部长度（毫米）</param>
/// <param name="WidthMM">内部宽度（毫米）</param>
/// <param name="HeightMM">内部高度（毫米）</param>
/// <param name="WallThicknessMM">壁厚/材料厚度（毫米）</param>
/// <param name="FlapDepthMM">翻盖深度（毫米，0表示自动计算）</param>
/// <param name="TabWidthMM">插舌宽度（毫米，0表示自动计算）</param>
/// <param name="Lock">锁扣类型</param>
/// <param name="AutoCalculateFlap">是否自动计算翻盖尺寸</param>
public record BoxParameters(
    BoxType Type,
    float LengthMM,
    float WidthMM,
    float HeightMM,
    float WallThicknessMM = 2.0f,
    float FlapDepthMM = 0f,
    float TabWidthMM = 0f,
    LockType Lock = LockType.None,
    bool AutoCalculateFlap = true
)
{
    /// <summary>
    /// 获取实际翻盖深度（自动计算或使用指定值）
    /// </summary>
    public float GetFlapDepth()
    {
        if (!AutoCalculateFlap && FlapDepthMM > 0)
            return FlapDepthMM;

        // 自动计算：翻盖深度 = 高度的50%-70%
        return HeightMM * 0.6f;
    }

    /// <summary>
    /// 获取实际插舌宽度（自动计算或使用指定值）
    /// </summary>
    public float GetTabWidth()
    {
        if (!AutoCalculateFlap && TabWidthMM > 0)
            return TabWidthMM;

        // 自动计算：插舌宽度 = 宽度的30%-40%
        return WidthMM * 0.35f;
    }
};
