using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Numerics;
using BoxCanvasDesigner.Dieline;

namespace BoxCanvasDesigner.Export;

/// <summary>
/// PDF刀版导出器
/// </summary>
public class PdfExporter
{
    private const float MM_TO_POINTS = 2.83465f; // 1mm = 2.83465 points (PDF单位)

    /// <summary>
    /// 导出刀版为PDF文件
    /// </summary>
    public void ExportDieline(DielineData dieline, string filePath, BoxParameters parameters)
    {
        // 创建PDF文档
        PdfDocument document = new PdfDocument();
        document.Info.Title = $"刀版 - {parameters.Type}";
        document.Info.Author = "3D Box Canvas Designer";
        document.Info.Subject = $"尺寸: {parameters.LengthMM}×{parameters.WidthMM}×{parameters.HeightMM}mm";

        // 添加页面
        PdfPage page = document.AddPage();

        // 设置页面尺寸（A3横向，足够大）
        page.Width = XUnit.FromMillimeter(420);
        page.Height = XUnit.FromMillimeter(297);

        // 获取绘图对象
        XGraphics gfx = XGraphics.FromPdfPage(page);

        // 计算缩放和偏移（使刀版居中）
        float scale = CalculateScale(dieline.Bounds, page);
        float offsetX = 50; // 左边距50点
        float offsetY = 50; // 上边距50点

        // 绘制标题（使用XStringFormats，不创建XFont对象）
        DrawTitle(gfx, parameters, offsetX, 20);

        // 绘制切割线（实线，黑色）
        XPen cutPen = new XPen(XColors.Black, 0.5);
        foreach (var line in dieline.CutLines)
        {
            DrawLine(gfx, line, cutPen, scale, offsetX, offsetY);
        }

        // 绘制折叠线（虚线，蓝色）
        XPen foldPen = new XPen(XColors.Blue, 0.5);
        foldPen.DashStyle = XDashStyle.Dash;
        foreach (var line in dieline.FoldLines)
        {
            DrawLine(gfx, line, foldPen, scale, offsetX, offsetY);
        }

        // 绘制粘合区（浅灰色填充）
        XBrush glueBrush = new XSolidBrush(XColor.FromArgb(50, 200, 200, 200));
        foreach (var glueArea in dieline.GlueAreas)
        {
            DrawPolygon(gfx, glueArea, glueBrush, scale, offsetX, offsetY);
        }

        // 绘制面板标签
        DrawPanelLabels(gfx, dieline.Panels, scale, offsetX, offsetY);

        // 绘制尺寸标注
        DrawDimensions(gfx, dieline, parameters, scale, offsetX, offsetY);

        // 绘制图例
        DrawLegend(gfx, page.Width.Point - 150, 50);

        // 保存文件
        document.Save(filePath);
        Console.WriteLine($"PDF刀版已导出: {filePath}");
    }

    /// <summary>
    /// 计算缩放比例（使刀版适应页面）
    /// </summary>
    private float CalculateScale(BoundingBox2D bounds, PdfPage page)
    {
        float pageWidthMM = (float)page.Width.Millimeter - 100; // 留100mm边距
        float pageHeightMM = (float)page.Height.Millimeter - 100;

        float scaleX = pageWidthMM / bounds.Width;
        float scaleY = pageHeightMM / bounds.Height;

        return Math.Min(scaleX, scaleY) * 0.9f; // 再缩小10%留白
    }

    /// <summary>
    /// 绘制标题
    /// </summary>
    private void DrawTitle(XGraphics gfx, BoxParameters parameters, float x, float y)
    {
        string title = $"Dieline - {GetBoxTypeName(parameters.Type)}";
        string info = $"Size: {parameters.LengthMM} x {parameters.WidthMM} x {parameters.HeightMM} mm  |  Wall: {parameters.WallThicknessMM} mm";

        // 使用PDF内置字体
        XFont titleFont = new XFont("Helvetica", 14, XFontStyleEx.Bold, new XPdfFontOptions(PdfFontEncoding.WinAnsi));
        XFont infoFont = new XFont("Helvetica", 10, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.WinAnsi));

        gfx.DrawString(title, titleFont, XBrushes.Black, x, y);
        gfx.DrawString(info, infoFont, XBrushes.Gray, x, y + 20);
    }

    /// <summary>
    /// 绘制面板标签（批量处理）
    /// </summary>
    private void DrawPanelLabels(XGraphics gfx, List<Panel> panels, float scale, float offsetX, float offsetY)
    {
        XFont font = new XFont("Helvetica", 8, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.WinAnsi));
        XBrush brush = XBrushes.Red;

        foreach (var panel in panels)
        {
            float x = panel.Center.X * scale + offsetX;
            float y = panel.Center.Y * scale + offsetY;

            string label = $"{panel.Name} {panel.WidthMM:F0}x{panel.HeightMM:F0}mm";
            XSize size = gfx.MeasureString(label, font);
            gfx.DrawString(label, font, brush, x - size.Width / 2, y);
        }
    }

    /// <summary>
    /// 绘制线段
    /// </summary>
    private void DrawLine(XGraphics gfx, Line2D line, XPen pen, float scale, float offsetX, float offsetY)
    {
        float x1 = line.Start.X * scale + offsetX;
        float y1 = line.Start.Y * scale + offsetY;
        float x2 = line.End.X * scale + offsetX;
        float y2 = line.End.Y * scale + offsetY;

        gfx.DrawLine(pen, x1, y1, x2, y2);
    }

    /// <summary>
    /// 绘制多边形
    /// </summary>
    private void DrawPolygon(XGraphics gfx, Polygon2D polygon, XBrush brush, float scale, float offsetX, float offsetY)
    {
        if (polygon.Points == null || polygon.Points.Count < 3)
            return;

        XPoint[] points = new XPoint[polygon.Points.Count];
        for (int i = 0; i < polygon.Points.Count; i++)
        {
            points[i] = new XPoint(
                polygon.Points[i].X * scale + offsetX,
                polygon.Points[i].Y * scale + offsetY
            );
        }

        gfx.DrawPolygon(brush, points, XFillMode.Winding);
    }

    /// <summary>
    /// 绘制面板标签
    /// </summary>
    private void DrawPanelLabel(XGraphics gfx, Panel panel, XFont font, XBrush brush, float scale, float offsetX, float offsetY)
    {
        float x = panel.Center.X * scale + offsetX;
        float y = panel.Center.Y * scale + offsetY;

        string label = $"{panel.Name}\n{panel.WidthMM:F0}×{panel.HeightMM:F0}mm";

        XSize size = gfx.MeasureString(label, font);
        gfx.DrawString(label, font, brush, x - size.Width / 2, y);
    }

    /// <summary>
    /// 绘制尺寸标注
    /// </summary>
    private void DrawDimensions(XGraphics gfx, DielineData dieline, BoxParameters parameters, float scale, float offsetX, float offsetY)
    {
        XPen dimPen = new XPen(XColors.DarkGray, 0.3);
        XFont dimFont = new XFont("Helvetica", 7, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.WinAnsi));
        XBrush dimBrush = XBrushes.DarkGray;

        // 在边界框外侧标注总宽度和总高度
        float minX = dieline.Bounds.Min.X * scale + offsetX;
        float minY = dieline.Bounds.Min.Y * scale + offsetY;
        float maxX = dieline.Bounds.Max.X * scale + offsetX;
        float maxY = dieline.Bounds.Max.Y * scale + offsetY;

        // 宽度标注
        gfx.DrawLine(dimPen, minX, maxY + 20, maxX, maxY + 20);
        gfx.DrawLine(dimPen, minX, maxY + 15, minX, maxY + 25);
        gfx.DrawLine(dimPen, maxX, maxY + 15, maxX, maxY + 25);
        string widthLabel = $"{dieline.Bounds.Width:F1} mm";
        gfx.DrawString(widthLabel, dimFont, dimBrush, (minX + maxX) / 2 - 20, maxY + 30);

        // 高度标注
        gfx.DrawLine(dimPen, maxX + 20, minY, maxX + 20, maxY);
        gfx.DrawLine(dimPen, maxX + 15, minY, maxX + 25, minY);
        gfx.DrawLine(dimPen, maxX + 15, maxY, maxX + 25, maxY);
        string heightLabel = $"{dieline.Bounds.Height:F1} mm";
        gfx.DrawString(heightLabel, dimFont, dimBrush, maxX + 30, (minY + maxY) / 2);
    }

    /// <summary>
    /// 绘制图例
    /// </summary>
    private void DrawLegend(XGraphics gfx, double x, double y)
    {
        XFont legendFont = new XFont("Helvetica", 9, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.WinAnsi));
        XPen cutPen = new XPen(XColors.Black, 1);
        XPen foldPen = new XPen(XColors.Blue, 1);
        foldPen.DashStyle = XDashStyle.Dash;

        gfx.DrawString("Legend:", legendFont, XBrushes.Black, x, y);

        gfx.DrawLine(cutPen, x, y + 15, x + 30, y + 15);
        gfx.DrawString("Cut Line", legendFont, XBrushes.Black, x + 35, y + 18);

        gfx.DrawLine(foldPen, x, y + 30, x + 30, y + 30);
        gfx.DrawString("Fold Line", legendFont, XBrushes.Black, x + 35, y + 33);

        XBrush glueBrush = new XSolidBrush(XColor.FromArgb(100, 200, 200, 200));
        gfx.DrawRectangle(glueBrush, x, y + 40, 30, 10);
        gfx.DrawString("Glue Area", legendFont, XBrushes.Black, x + 35, y + 48);
    }

    /// <summary>
    /// 获取盒型中文名称
    /// </summary>
    private string GetBoxTypeName(BoxType type)
    {
        return type switch
        {
            BoxType.TuckEnd => "插口盒",
            BoxType.Mailer => "邮寄盒/飞机盒",
            BoxType.CorrugatedRSC => "瓦楞标准开槽箱",
            _ => type.ToString()
        };
    }
}
