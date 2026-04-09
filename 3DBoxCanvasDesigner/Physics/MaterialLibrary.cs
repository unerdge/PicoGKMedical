namespace BoxCanvasDesigner.Physics;

/// <summary>
/// 材料库 - 预定义材料数据
/// </summary>
public class MaterialLibrary
{
    private static readonly List<MaterialInfo> _materials = new()
    {
        // 白卡纸系列
        new MaterialInfo("白卡纸250gsm", MaterialType.WhiteCardboard, 0.35f, 8.0f, 250f, 0.5f, 12f),
        new MaterialInfo("白卡纸300gsm", MaterialType.WhiteCardboard, 0.42f, 10.0f, 300f, 1.0f, 15f),
        new MaterialInfo("白卡纸350gsm", MaterialType.WhiteCardboard, 0.49f, 12.0f, 350f, 1.5f, 18f),
        new MaterialInfo("白卡纸400gsm", MaterialType.WhiteCardboard, 0.56f, 14.0f, 400f, 2.0f, 22f),

        // 灰底白板纸系列
        new MaterialInfo("灰底白板250gsm", MaterialType.GrayCardboard, 0.35f, 8.5f, 250f, 0.5f, 10f),
        new MaterialInfo("灰底白板300gsm", MaterialType.GrayCardboard, 0.42f, 10.5f, 300f, 1.2f, 12f),
        new MaterialInfo("灰底白板350gsm", MaterialType.GrayCardboard, 0.49f, 12.5f, 350f, 1.8f, 14f),
        new MaterialInfo("灰底白板450gsm", MaterialType.GrayCardboard, 0.63f, 16.0f, 450f, 2.5f, 18f),

        // 牛皮卡纸系列
        new MaterialInfo("牛皮卡纸200gsm", MaterialType.KraftPaper, 0.28f, 9.0f, 200f, 0.5f, 8f),
        new MaterialInfo("牛皮卡纸300gsm", MaterialType.KraftPaper, 0.42f, 13.0f, 300f, 1.5f, 11f),
        new MaterialInfo("牛皮卡纸400gsm", MaterialType.KraftPaper, 0.56f, 17.0f, 400f, 3.0f, 15f),

        // 瓦楞纸板系列
        new MaterialInfo("瓦楞E楞", MaterialType.CorrugatedE, 1.5f, 23.0f, 450f, 8.0f, 6f),
        new MaterialInfo("瓦楞B楞", MaterialType.CorrugatedB, 3.0f, 32.0f, 650f, 15.0f, 8f),
        new MaterialInfo("瓦楞BC双层", MaterialType.CorrugatedBC, 6.0f, 48.0f, 900f, 30.0f, 12f)
    };

    /// <summary>
    /// 获取所有材料
    /// </summary>
    public static IReadOnlyList<MaterialInfo> GetAllMaterials() => _materials;

    /// <summary>
    /// 根据类型筛选材料
    /// </summary>
    public static List<MaterialInfo> GetMaterialsByType(MaterialType type)
    {
        return _materials.Where(m => m.Type == type).ToList();
    }

    /// <summary>
    /// 根据承重要求推荐材料
    /// </summary>
    public static List<MaterialInfo> GetMaterialsByWeight(float productWeightKg)
    {
        return _materials.Where(m => m.MaxLoadKg >= productWeightKg).OrderBy(m => m.CostPerSqM).ToList();
    }

    /// <summary>
    /// 根据名称查找材料
    /// </summary>
    public static MaterialInfo? GetMaterialByName(string name)
    {
        return _materials.FirstOrDefault(m => m.Name == name);
    }

    /// <summary>
    /// 获取最经济的材料（满足承重要求）
    /// </summary>
    public static MaterialInfo? GetMostEconomicalMaterial(float productWeightKg)
    {
        return _materials
            .Where(m => m.MaxLoadKg >= productWeightKg)
            .OrderBy(m => m.CostPerSqM)
            .FirstOrDefault();
    }

    /// <summary>
    /// 获取推荐材料（性价比最优）
    /// </summary>
    public static MaterialInfo? GetRecommendedMaterial(float productWeightKg, BoxParameters parameters)
    {
        // 计算盒子周长
        float perimeterMM = 2 * (parameters.LengthMM + parameters.WidthMM);

        // 根据尺寸和重量选择材料
        if (productWeightKg > 10f || perimeterMM > 800f)
        {
            // 大尺寸或重物 -> 瓦楞纸板
            return _materials.FirstOrDefault(m => m.Type == MaterialType.CorrugatedB && m.MaxLoadKg >= productWeightKg);
        }
        else if (productWeightKg > 3f || perimeterMM > 500f)
        {
            // 中等尺寸 -> E楞或厚卡纸
            return _materials.FirstOrDefault(m =>
                (m.Type == MaterialType.CorrugatedE || m.WeightGSM >= 350f) &&
                m.MaxLoadKg >= productWeightKg);
        }
        else
        {
            // 小尺寸轻物 -> 卡纸
            return _materials
                .Where(m => m.Type != MaterialType.CorrugatedB &&
                           m.Type != MaterialType.CorrugatedBC &&
                           m.MaxLoadKg >= productWeightKg)
                .OrderBy(m => m.CostPerSqM)
                .FirstOrDefault();
        }
    }
}
