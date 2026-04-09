using PicoGK;
using System.Text;

namespace BoxCanvasDesigner.Export;

/// <summary>
/// STL 3D模型导出器（用于3D打印）
/// </summary>
public class StlExporter
{
    /// <summary>
    /// 导出盒子3D模型为STL文件
    /// </summary>
    public void ExportBox(Voxels voxels, string fileName, BoxParameters parameters)
    {
        // 从体素生成网格
        Mesh mesh = new Mesh(voxels);

        // 导出为ASCII STL格式
        ExportMeshToStl(mesh, fileName, parameters);
    }

    private void ExportMeshToStl(Mesh mesh, string fileName, BoxParameters parameters)
    {
        var stl = new StringBuilder();

        // STL文件头
        stl.AppendLine($"solid {parameters.Type}_Box");

        // 遍历所有三角形
        int triangleCount = mesh.nTriangleCount();
        for (int i = 0; i < triangleCount; i++)
        {
            mesh.GetTriangle(i, out var v0, out var v1, out var v2);

            // 计算法向量
            var edge1 = v1 - v0;
            var edge2 = v2 - v0;
            var normal = System.Numerics.Vector3.Normalize(
                System.Numerics.Vector3.Cross(
                    new System.Numerics.Vector3(edge1.X, edge1.Y, edge1.Z),
                    new System.Numerics.Vector3(edge2.X, edge2.Y, edge2.Z)
                )
            );

            // 写入三角形
            stl.AppendLine($"  facet normal {normal.X:E} {normal.Y:E} {normal.Z:E}");
            stl.AppendLine("    outer loop");
            stl.AppendLine($"      vertex {v0.X:E} {v0.Y:E} {v0.Z:E}");
            stl.AppendLine($"      vertex {v1.X:E} {v1.Y:E} {v1.Z:E}");
            stl.AppendLine($"      vertex {v2.X:E} {v2.Y:E} {v2.Z:E}");
            stl.AppendLine("    endloop");
            stl.AppendLine("  endfacet");
        }

        stl.AppendLine($"endsolid {parameters.Type}_Box");

        File.WriteAllText(fileName, stl.ToString());
    }
}
