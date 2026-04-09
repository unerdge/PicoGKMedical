using BoxCanvasDesigner.Dieline;
using System.Text;

namespace BoxCanvasDesigner.Export;

/// <summary>
/// DXF格式导出器（用于模切机）
/// 简化版DXF R12格式，无需外部库
/// </summary>
public class DxfExporter
{
    /// <summary>
    /// 导出刀版为DXF文件（图层分离：CUT、FOLD、GLUE）
    /// </summary>
    public void ExportDieline(DielineData dieline, string fileName, BoxParameters parameters)
    {
        var dxf = new StringBuilder();

        // DXF文件头
        WriteDxfHeader(dxf);

        // 定义图层
        WriteDxfLayers(dxf);

        // 实体部分
        dxf.AppendLine("0\nSECTION");
        dxf.AppendLine("2\nENTITIES");

        // 切割线 -> CUT图层
        foreach (var line in dieline.CutLines)
        {
            WriteDxfLine(dxf, line, "CUT");
        }

        // 折叠线 -> FOLD图层
        foreach (var line in dieline.FoldLines)
        {
            WriteDxfLine(dxf, line, "FOLD");
        }

        // 粘合区域 -> GLUE图层
        foreach (var glueArea in dieline.GlueAreas)
        {
            WriteDxfPolyline(dxf, glueArea, "GLUE");
        }

        dxf.AppendLine("0\nENDSEC");

        // 文件结束
        dxf.AppendLine("0\nEOF");

        File.WriteAllText(fileName, dxf.ToString());
    }

    private void WriteDxfHeader(StringBuilder dxf)
    {
        dxf.AppendLine("0\nSECTION");
        dxf.AppendLine("2\nHEADER");
        dxf.AppendLine("9\n$ACADVER");
        dxf.AppendLine("1\nAC1009"); // DXF R12
        dxf.AppendLine("9\n$INSUNITS");
        dxf.AppendLine("70\n4"); // 毫米
        dxf.AppendLine("0\nENDSEC");
    }

    private void WriteDxfLayers(StringBuilder dxf)
    {
        dxf.AppendLine("0\nSECTION");
        dxf.AppendLine("2\nTABLES");
        dxf.AppendLine("0\nTABLE");
        dxf.AppendLine("2\nLAYER");
        dxf.AppendLine("70\n3"); // 3个图层

        // CUT图层（黑色）
        dxf.AppendLine("0\nLAYER");
        dxf.AppendLine("2\nCUT");
        dxf.AppendLine("70\n0");
        dxf.AppendLine("62\n7"); // 白色/黑色
        dxf.AppendLine("6\nCONTINUOUS");

        // FOLD图层（蓝色虚线）
        dxf.AppendLine("0\nLAYER");
        dxf.AppendLine("2\nFOLD");
        dxf.AppendLine("70\n0");
        dxf.AppendLine("62\n5"); // 蓝色
        dxf.AppendLine("6\nDASHED");

        // GLUE图层（灰色）
        dxf.AppendLine("0\nLAYER");
        dxf.AppendLine("2\nGLUE");
        dxf.AppendLine("70\n0");
        dxf.AppendLine("62\n8"); // 灰色
        dxf.AppendLine("6\nCONTINUOUS");

        dxf.AppendLine("0\nENDTAB");
        dxf.AppendLine("0\nENDSEC");
    }

    private void WriteDxfLine(StringBuilder dxf, Line2D line, string layer)
    {
        dxf.AppendLine("0\nLINE");
        dxf.AppendLine($"8\n{layer}");
        dxf.AppendLine($"10\n{line.Start.X:F3}");
        dxf.AppendLine($"20\n{line.Start.Y:F3}");
        dxf.AppendLine("30\n0.0");
        dxf.AppendLine($"11\n{line.End.X:F3}");
        dxf.AppendLine($"21\n{line.End.Y:F3}");
        dxf.AppendLine("31\n0.0");
    }

    private void WriteDxfPolyline(StringBuilder dxf, Polygon2D polygon, string layer)
    {
        dxf.AppendLine("0\nPOLYLINE");
        dxf.AppendLine($"8\n{layer}");
        dxf.AppendLine("66\n1");
        dxf.AppendLine("70\n1"); // 闭合

        foreach (var pt in polygon.Points)
        {
            dxf.AppendLine("0\nVERTEX");
            dxf.AppendLine($"8\n{layer}");
            dxf.AppendLine($"10\n{pt.X:F3}");
            dxf.AppendLine($"20\n{pt.Y:F3}");
            dxf.AppendLine("30\n0.0");
        }

        dxf.AppendLine("0\nSEQEND");
    }
}
