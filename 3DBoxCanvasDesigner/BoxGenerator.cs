using PicoGK;
using System.Numerics;

namespace BoxCanvasDesigner;

/// <summary>
/// 盒型几何生成器
/// </summary>
public class BoxGenerator
{
    private readonly BoxParameters _params;

    public BoxGenerator(BoxParameters parameters)
    {
        _params = parameters;
    }

    /// <summary>
    /// 生成盒子的体素几何（实心壁面）
    /// </summary>
    public Voxels GenerateVoxels()
    {
        float L = _params.LengthMM;
        float W = _params.WidthMM;
        float H = _params.HeightMM;
        float wall = _params.WallThicknessMM;

        Mesh mesh = new Mesh();

        // 生成6个面的网格（每个面是实心壁板）
        AddBottomPanel(ref mesh, L, W, wall);
        AddTopPanel(ref mesh, L, W, H, wall);
        AddFrontPanel(ref mesh, L, H, W, wall);
        AddBackPanel(ref mesh, L, H, W, wall);
        AddLeftPanel(ref mesh, W, H, L, wall);
        AddRightPanel(ref mesh, W, H, L, wall);

        return new Voxels(mesh);
    }

    /// <summary>
    /// 底面板（Z=0平面，厚度向上）
    /// </summary>
    private void AddBottomPanel(ref Mesh mesh, float length, float width, float thickness)
    {
        float halfL = length / 2;
        float halfW = width / 2;

        // 外表面（Z=0）
        Vector3 p0 = new Vector3(-halfL, -halfW, 0);
        Vector3 p1 = new Vector3(halfL, -halfW, 0);
        Vector3 p2 = new Vector3(halfL, halfW, 0);
        Vector3 p3 = new Vector3(-halfL, halfW, 0);

        // 内表面（Z=thickness）
        Vector3 p4 = new Vector3(-halfL, -halfW, thickness);
        Vector3 p5 = new Vector3(halfL, -halfW, thickness);
        Vector3 p6 = new Vector3(halfL, halfW, thickness);
        Vector3 p7 = new Vector3(-halfL, halfW, thickness);

        // 外表面（朝下）
        mesh.nAddTriangle(p0, p2, p1);
        mesh.nAddTriangle(p0, p3, p2);

        // 内表面（朝上）
        mesh.nAddTriangle(p4, p5, p6);
        mesh.nAddTriangle(p4, p6, p7);

        // 四条边的侧面
        AddQuad(ref mesh, p0, p1, p5, p4); // 前边
        AddQuad(ref mesh, p1, p2, p6, p5); // 右边
        AddQuad(ref mesh, p2, p3, p7, p6); // 后边
        AddQuad(ref mesh, p3, p0, p4, p7); // 左边
    }

    /// <summary>
    /// 顶面板（Z=H平面，厚度向下）
    /// </summary>
    private void AddTopPanel(ref Mesh mesh, float length, float width, float height, float thickness)
    {
        float halfL = length / 2;
        float halfW = width / 2;

        // 外表面（Z=height）
        Vector3 p0 = new Vector3(-halfL, -halfW, height);
        Vector3 p1 = new Vector3(halfL, -halfW, height);
        Vector3 p2 = new Vector3(halfL, halfW, height);
        Vector3 p3 = new Vector3(-halfL, halfW, height);

        // 内表面（Z=height-thickness）
        Vector3 p4 = new Vector3(-halfL, -halfW, height - thickness);
        Vector3 p5 = new Vector3(halfL, -halfW, height - thickness);
        Vector3 p6 = new Vector3(halfL, halfW, height - thickness);
        Vector3 p7 = new Vector3(-halfL, halfW, height - thickness);

        // 外表面（朝上）
        mesh.nAddTriangle(p0, p1, p2);
        mesh.nAddTriangle(p0, p2, p3);

        // 内表面（朝下）
        mesh.nAddTriangle(p4, p6, p5);
        mesh.nAddTriangle(p4, p7, p6);

        // 四条边的侧面
        AddQuad(ref mesh, p0, p4, p5, p1);
        AddQuad(ref mesh, p1, p5, p6, p2);
        AddQuad(ref mesh, p2, p6, p7, p3);
        AddQuad(ref mesh, p3, p7, p4, p0);
    }

    /// <summary>
    /// 前面板（Y=-W/2平面，厚度向内）
    /// </summary>
    private void AddFrontPanel(ref Mesh mesh, float length, float height, float width, float thickness)
    {
        float halfL = length / 2;
        float halfW = width / 2;

        // 外表面
        Vector3 p0 = new Vector3(-halfL, -halfW, 0);
        Vector3 p1 = new Vector3(halfL, -halfW, 0);
        Vector3 p2 = new Vector3(halfL, -halfW, height);
        Vector3 p3 = new Vector3(-halfL, -halfW, height);

        // 内表面
        Vector3 p4 = new Vector3(-halfL, -halfW + thickness, 0);
        Vector3 p5 = new Vector3(halfL, -halfW + thickness, 0);
        Vector3 p6 = new Vector3(halfL, -halfW + thickness, height);
        Vector3 p7 = new Vector3(-halfL, -halfW + thickness, height);

        // 外表面（朝外）
        mesh.nAddTriangle(p0, p1, p2);
        mesh.nAddTriangle(p0, p2, p3);

        // 内表面（朝内）
        mesh.nAddTriangle(p4, p6, p5);
        mesh.nAddTriangle(p4, p7, p6);

        // 四条边
        AddQuad(ref mesh, p0, p4, p7, p3);
        AddQuad(ref mesh, p1, p2, p6, p5);
        AddQuad(ref mesh, p0, p3, p2, p1); // 底边（已被底面板覆盖，但为了封闭性）
        AddQuad(ref mesh, p4, p5, p6, p7); // 顶边
    }

    /// <summary>
    /// 后面板（Y=W/2平面，厚度向内）
    /// </summary>
    private void AddBackPanel(ref Mesh mesh, float length, float height, float width, float thickness)
    {
        float halfL = length / 2;
        float halfW = width / 2;

        Vector3 p0 = new Vector3(-halfL, halfW, 0);
        Vector3 p1 = new Vector3(halfL, halfW, 0);
        Vector3 p2 = new Vector3(halfL, halfW, height);
        Vector3 p3 = new Vector3(-halfL, halfW, height);

        Vector3 p4 = new Vector3(-halfL, halfW - thickness, 0);
        Vector3 p5 = new Vector3(halfL, halfW - thickness, 0);
        Vector3 p6 = new Vector3(halfL, halfW - thickness, height);
        Vector3 p7 = new Vector3(-halfL, halfW - thickness, height);

        mesh.nAddTriangle(p0, p2, p1);
        mesh.nAddTriangle(p0, p3, p2);

        mesh.nAddTriangle(p4, p5, p6);
        mesh.nAddTriangle(p4, p6, p7);

        AddQuad(ref mesh, p0, p3, p7, p4);
        AddQuad(ref mesh, p1, p5, p6, p2);
    }

    /// <summary>
    /// 左面板（X=-L/2平面，厚度向内）
    /// </summary>
    private void AddLeftPanel(ref Mesh mesh, float width, float height, float length, float thickness)
    {
        float halfL = length / 2;
        float halfW = width / 2;

        Vector3 p0 = new Vector3(-halfL, -halfW, 0);
        Vector3 p1 = new Vector3(-halfL, halfW, 0);
        Vector3 p2 = new Vector3(-halfL, halfW, height);
        Vector3 p3 = new Vector3(-halfL, -halfW, height);

        Vector3 p4 = new Vector3(-halfL + thickness, -halfW, 0);
        Vector3 p5 = new Vector3(-halfL + thickness, halfW, 0);
        Vector3 p6 = new Vector3(-halfL + thickness, halfW, height);
        Vector3 p7 = new Vector3(-halfL + thickness, -halfW, height);

        mesh.nAddTriangle(p0, p2, p1);
        mesh.nAddTriangle(p0, p3, p2);

        mesh.nAddTriangle(p4, p5, p6);
        mesh.nAddTriangle(p4, p6, p7);
    }

    /// <summary>
    /// 右面板（X=L/2平面，厚度向内）
    /// </summary>
    private void AddRightPanel(ref Mesh mesh, float width, float height, float length, float thickness)
    {
        float halfL = length / 2;
        float halfW = width / 2;

        Vector3 p0 = new Vector3(halfL, -halfW, 0);
        Vector3 p1 = new Vector3(halfL, halfW, 0);
        Vector3 p2 = new Vector3(halfL, halfW, height);
        Vector3 p3 = new Vector3(halfL, -halfW, height);

        Vector3 p4 = new Vector3(halfL - thickness, -halfW, 0);
        Vector3 p5 = new Vector3(halfL - thickness, halfW, 0);
        Vector3 p6 = new Vector3(halfL - thickness, halfW, height);
        Vector3 p7 = new Vector3(halfL - thickness, -halfW, height);

        mesh.nAddTriangle(p0, p1, p2);
        mesh.nAddTriangle(p0, p2, p3);

        mesh.nAddTriangle(p4, p6, p5);
        mesh.nAddTriangle(p4, p7, p6);
    }

    /// <summary>
    /// 添加四边形（两个三角形）
    /// </summary>
    private void AddQuad(ref Mesh mesh, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        mesh.nAddTriangle(p0, p1, p2);
        mesh.nAddTriangle(p0, p2, p3);
    }

    /// <summary>
    /// 预览盒子（添加到PicoGK查看器）
    /// </summary>
    public void Preview()
    {
        try
        {
            // 设置材质颜色
            Library.oViewer().SetGroupMaterial(0, "FF6B35", 0.0f, 1.0f); // 橙色

            Voxels voxBox = GenerateVoxels();
            Library.oViewer().Add(voxBox);

            string typeStr = _params.Type switch
            {
                BoxType.TuckEnd => "插舌式盒",
                BoxType.Mailer => "邮寄盒",
                BoxType.CorrugatedRSC => "瓦楞标准开槽箱",
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
