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
/// 盒型参数
/// </summary>
/// <param name="Type">盒型类型</param>
/// <param name="LengthMM">内部长度（毫米）</param>
/// <param name="WidthMM">内部宽度（毫米）</param>
/// <param name="HeightMM">内部高度（毫米）</param>
/// <param name="WallThicknessMM">壁厚/材料厚度（毫米）</param>
public record BoxParameters(
    BoxType Type,
    float LengthMM,
    float WidthMM,
    float HeightMM,
    float WallThicknessMM = 2.0f
);
