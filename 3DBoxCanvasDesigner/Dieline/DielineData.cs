using System.Numerics;

namespace BoxCanvasDesigner.Dieline;

/// <summary>
/// 刀版数据结构
/// </summary>
public class DielineData
{
    /// <summary>切割线（实线）</summary>
    public List<Line2D> CutLines { get; set; } = new();

    /// <summary>折叠线（虚线）</summary>
    public List<Line2D> FoldLines { get; set; } = new();

    /// <summary>粘合区域</summary>
    public List<Polygon2D> GlueAreas { get; set; } = new();

    /// <summary>面板（正面/背面/侧面等）</summary>
    public List<Panel> Panels { get; set; } = new();

    /// <summary>边界框</summary>
    public BoundingBox2D Bounds { get; set; }

    /// <summary>总面积（平方毫米）</summary>
    public float TotalAreaMM2 => CalculateTotalArea();

    private float CalculateTotalArea()
    {
        float area = 0;
        foreach (var panel in Panels)
        {
            area += panel.CalculateArea();
        }
        return area;
    }
}

/// <summary>
/// 2D线段
/// </summary>
public struct Line2D
{
    public Vector2 Start;
    public Vector2 End;
    public LineType Type;

    public Line2D(Vector2 start, Vector2 end, LineType type = LineType.Cut)
    {
        Start = start;
        End = end;
        Type = type;
    }

    public float Length => Vector2.Distance(Start, End);
}

/// <summary>
/// 线段类型
/// </summary>
public enum LineType
{
    Cut,         // 切割线（实线）
    Fold,        // 折叠线（虚线）
    Perforation  // 穿孔线（点线）
}

/// <summary>
/// 2D多边形
/// </summary>
public struct Polygon2D
{
    public List<Vector2> Points;

    public Polygon2D(List<Vector2> points)
    {
        Points = points;
    }

    /// <summary>
    /// 计算多边形面积（使用鞋带公式）
    /// </summary>
    public float CalculateArea()
    {
        if (Points == null || Points.Count < 3)
            return 0;

        float area = 0;
        for (int i = 0; i < Points.Count; i++)
        {
            int j = (i + 1) % Points.Count;
            area += Points[i].X * Points[j].Y;
            area -= Points[j].X * Points[i].Y;
        }
        return MathF.Abs(area / 2f);
    }
}

/// <summary>
/// 面板（盒子的一个面）
/// </summary>
public class Panel
{
    /// <summary>面板名称</summary>
    public string Name { get; set; } = "";

    /// <summary>面板类型</summary>
    public PanelType Type { get; set; }

    /// <summary>边界多边形</summary>
    public Polygon2D Boundary { get; set; }

    /// <summary>中心点</summary>
    public Vector2 Center { get; set; }

    /// <summary>宽度（毫米）</summary>
    public float WidthMM { get; set; }

    /// <summary>高度（毫米）</summary>
    public float HeightMM { get; set; }

    public Panel(string name, PanelType type, Polygon2D boundary)
    {
        Name = name;
        Type = type;
        Boundary = boundary;
        Center = CalculateCenter();
    }

    private Vector2 CalculateCenter()
    {
        if (Boundary.Points == null || Boundary.Points.Count == 0)
            return Vector2.Zero;

        Vector2 sum = Vector2.Zero;
        foreach (var point in Boundary.Points)
        {
            sum += point;
        }
        return sum / Boundary.Points.Count;
    }

    public float CalculateArea() => Boundary.CalculateArea();
}

/// <summary>
/// 面板类型
/// </summary>
public enum PanelType
{
    Front,      // 正面
    Back,       // 背面
    Left,       // 左侧面
    Right,      // 右侧面
    Top,        // 顶面
    Bottom,     // 底面
    Flap,       // 翻盖
    Tab,        // 插舌
    Glue        // 粘合边
}

/// <summary>
/// 2D边界框
/// </summary>
public struct BoundingBox2D
{
    public Vector2 Min;
    public Vector2 Max;

    public BoundingBox2D(Vector2 min, Vector2 max)
    {
        Min = min;
        Max = max;
    }

    public float Width => Max.X - Min.X;
    public float Height => Max.Y - Min.Y;
    public Vector2 Center => (Min + Max) / 2f;

    /// <summary>
    /// 从点集合创建边界框
    /// </summary>
    public static BoundingBox2D FromPoints(IEnumerable<Vector2> points)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (var p in points)
        {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }

        return new BoundingBox2D(new Vector2(minX, minY), new Vector2(maxX, maxY));
    }
}
