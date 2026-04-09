using PicoGK;
using System.Numerics;
using Leap71.ShapeKernel;

namespace BoxCanvasDesigner;

/// <summary>
/// 盒型几何生成器（使用ShapeKernel简化实现）
/// </summary>
public class BoxGenerator
{
    private readonly BoxParameters _params;

    public BoxGenerator(BoxParameters parameters)
    {
        _params = parameters;
    }

    /// <summary>
    /// 生成盒子的体素几何（使用voxOffset创建空心壁面）
    /// </summary>
    public Voxels GenerateVoxels()
    {
        // RigidBox是两件式，需要特殊处理
        if (_params.Type == BoxType.RigidBox)
        {
            return GenerateRigidBoxVoxels();
        }

        // 其他盒型都是标准矩形盒子
        return GenerateStandardBoxVoxels();
    }

    /// <summary>
    /// 生成标准矩形盒子
    /// </summary>
    private Voxels GenerateStandardBoxVoxels()
    {
        float L = _params.LengthMM;
        float W = _params.WidthMM;
        float H = _params.HeightMM;
        float wall = _params.WallThicknessMM;

        // 创建外部盒子（包含壁厚的完整尺寸）
        LocalFrame oFrame = new LocalFrame(new Vector3(0, 0, H / 2), Vector3.UnitZ);
        BaseBox oBox = new BaseBox(oFrame, L + 2 * wall, W + 2 * wall, H);
        Voxels voxOuter = oBox.voxConstruct();

        // 使用voxOffset创建空心结构：外壳 - 内部 = 壁面
        // voxOffset(-wall)会向内收缩wall的距离，得到内部空腔
        Voxels voxInner = voxOuter.voxOffset(-wall);
        Voxels voxWalls = voxOuter - voxInner;

        return voxWalls;
    }

    /// <summary>
    /// 生成天地盖精装盒（两件式：盒盖+盒底）
    /// </summary>
    private Voxels GenerateRigidBoxVoxels()
    {
        float L = _params.LengthMM;
        float W = _params.WidthMM;
        float H = _params.HeightMM;
        float wall = _params.WallThicknessMM;

        float lidHeight = H * 0.4f;  // 盒盖高度
        float baseHeight = H * 0.6f; // 盒底高度
        float gap = 30f; // 盒盖和盒底之间的间距

        // 生成盒底（左侧）
        LocalFrame baseFrame = new LocalFrame(new Vector3(-gap / 2 - L / 2, 0, baseHeight / 2), Vector3.UnitZ);
        BaseBox baseBox = new BaseBox(baseFrame, L + 2 * wall, W + 2 * wall, baseHeight);
        Voxels voxBaseOuter = baseBox.voxConstruct();
        Voxels voxBaseInner = voxBaseOuter.voxOffset(-wall);
        Voxels voxBase = voxBaseOuter - voxBaseInner;

        // 生成盒盖（右侧）
        LocalFrame lidFrame = new LocalFrame(new Vector3(gap / 2 + L / 2, 0, lidHeight / 2), Vector3.UnitZ);
        BaseBox lidBox = new BaseBox(lidFrame, L + 2 * wall, W + 2 * wall, lidHeight);
        Voxels voxLidOuter = lidBox.voxConstruct();
        Voxels voxLidInner = voxLidOuter.voxOffset(-wall);
        Voxels voxLid = voxLidOuter - voxLidInner;

        // 合并盒底和盒盖
        return voxBase + voxLid;
    }

    /// <summary>
    /// 预览盒子（使用ShapeKernel的预览方法）
    /// </summary>
    public void Preview()
    {
        try
        {
            Voxels voxBox = GenerateVoxels();
            Sh.PreviewVoxels(voxBox, Cp.clrWarning); // 橙色

            string typeStr = _params.Type switch
            {
                BoxType.TuckEnd => "插舌式盒",
                BoxType.Mailer => "邮寄盒",
                BoxType.CorrugatedRSC => "瓦楞标准开槽箱",
                BoxType.AutoLockBottom => "自动锁底盒",
                BoxType.PillowBox => "枕头盒",
                BoxType.RigidBox => "天地盖精装盒",
                _ => "未知类型"
            };

            Library.Log($"盒型: {typeStr}");
            Library.Log($"内部尺寸: {_params.LengthMM} × {_params.WidthMM} × {_params.HeightMM} mm");
            Library.Log($"壁厚: {_params.WallThicknessMM} mm");
            Library.Log("3D盒型生成成功");
        }
        catch (Exception ex)
        {
            Library.Log($"生成错误: {ex.Message}");
        }
    }
}
