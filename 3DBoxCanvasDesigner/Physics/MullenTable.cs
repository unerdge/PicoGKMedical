namespace BoxCanvasDesigner.Physics;

/// <summary>
/// Mullen耐破强度查表器
/// </summary>
public class MullenTable
{
    /// <summary>
    /// Mullen等级定义
    /// </summary>
    public record MullenGrade(
        string Name,
        int MullenPSI,
        float MaxLoadKg,
        string TypicalUse
    );

    private static readonly List<MullenGrade> _grades = new()
    {
        new("125#", 125, 9f, "轻型零售盒"),
        new("150#", 150, 11f, "电子配件"),
        new("200#", 200, 16f, "标准快递"),
        new("275#", 275, 27f, "重型运输"),
        new("350#", 350, 36f, "工业包装")
    };

    /// <summary>
    /// 根据产品重量推荐Mullen等级
    /// </summary>
    public static MullenGrade GetRecommendedGrade(float productWeightKg)
    {
        foreach (var grade in _grades)
        {
            if (productWeightKg <= grade.MaxLoadKg)
                return grade;
        }
        return _grades[^1]; // 返回最高等级
    }

    /// <summary>
    /// 获取所有等级
    /// </summary>
    public static IReadOnlyList<MullenGrade> GetAllGrades() => _grades;

    /// <summary>
    /// 验证材料是否满足重量要求
    /// </summary>
    public static bool ValidateMaterial(MullenGrade grade, float productWeightKg)
    {
        return productWeightKg <= grade.MaxLoadKg;
    }
}
