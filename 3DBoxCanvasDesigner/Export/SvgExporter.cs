using BoxCanvasDesigner.Dieline;
using System.Text;

namespace BoxCanvasDesigner.Export;

/// <summary>
/// SVG矢量格式导出器
/// </summary>
public class SvgExporter
{
    /// <summary>
    /// 导出刀版为SVG文件
    /// </summary>
    public void ExportDieline(DielineData dieline, string fileName, BoxParameters parameters)
    {
        var bounds = dieline.Bounds;
        float margin = 20f;
        float viewWidth = bounds.Width + 2 * margin;
        float viewHeight = bounds.Height + 2 * margin;

        var svg = new StringBuilder();
        svg.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        svg.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{viewWidth}mm\" height=\"{viewHeight}mm\" viewBox=\"0 0 {viewWidth} {viewHeight}\">");

        // 定义样式
        svg.AppendLine("  <defs>");
        svg.AppendLine("    <style>");
        svg.AppendLine("      .cut-line { stroke: black; stroke-width: 0.5; fill: none; }");
        svg.AppendLine("      .fold-line { stroke: blue; stroke-width: 0.3; stroke-dasharray: 2,2; fill: none; }");
        svg.AppendLine("      .glue-area { fill: lightgray; fill-opacity: 0.3; stroke: gray; stroke-width: 0.2; }");
        svg.AppendLine("      .label-text { font-family: Arial; font-size: 8px; fill: black; }");
        svg.AppendLine("    </style>");
        svg.AppendLine("  </defs>");

        // 背景
        svg.AppendLine($"  <rect x=\"0\" y=\"0\" width=\"{viewWidth}\" height=\"{viewHeight}\" fill=\"white\"/>");

        // 偏移量（居中）
        float offsetX = margin - bounds.Min.X;
        float offsetY = margin - bounds.Min.Y;

        // 绘制粘合区域
        foreach (var glueArea in dieline.GlueAreas)
        {
            svg.Append($"  <polygon class=\"glue-area\" points=\"");
            foreach (var pt in glueArea.Points)
            {
                svg.Append($"{pt.X + offsetX},{pt.Y + offsetY} ");
            }
            svg.AppendLine("\"/>");
        }

        // 绘制折叠线
        foreach (var line in dieline.FoldLines)
        {
            svg.AppendLine($"  <line class=\"fold-line\" x1=\"{line.Start.X + offsetX}\" y1=\"{line.Start.Y + offsetY}\" x2=\"{line.End.X + offsetX}\" y2=\"{line.End.Y + offsetY}\"/>");
        }

        // 绘制切割线
        foreach (var line in dieline.CutLines)
        {
            svg.AppendLine($"  <line class=\"cut-line\" x1=\"{line.Start.X + offsetX}\" y1=\"{line.Start.Y + offsetY}\" x2=\"{line.End.X + offsetX}\" y2=\"{line.End.Y + offsetY}\"/>");
        }

        // 绘制面板标签
        foreach (var panel in dieline.Panels)
        {
            if (panel.Type != PanelType.Glue)
            {
                var center = panel.Center;
                svg.AppendLine($"  <text class=\"label-text\" x=\"{center.X + offsetX}\" y=\"{center.Y + offsetY}\" text-anchor=\"middle\">{panel.Name}</text>");
            }
        }

        // 标题
        svg.AppendLine($"  <text x=\"{viewWidth / 2}\" y=\"15\" font-size=\"12\" font-weight=\"bold\" text-anchor=\"middle\">{parameters.Type} Dieline</text>");

        svg.AppendLine("</svg>");

        File.WriteAllText(fileName, svg.ToString());
    }
}
