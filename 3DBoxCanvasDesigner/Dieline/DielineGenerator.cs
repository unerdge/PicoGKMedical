using System.Numerics;

namespace BoxCanvasDesigner.Dieline;

/// <summary>
/// 刀版生成器 - 将3D盒型参数转换为2D平面刀版
/// </summary>
public class DielineGenerator
{
    /// <summary>
    /// 生成刀版
    /// </summary>
    public DielineData GenerateDieline(BoxParameters parameters)
    {
        return parameters.Type switch
        {
            BoxType.TuckEnd => GenerateTuckEndDieline(parameters),
            BoxType.Mailer => GenerateMailerDieline(parameters),
            BoxType.CorrugatedRSC => GenerateCorrugatedRSCDieline(parameters),
            BoxType.AutoLockBottom => GenerateAutoLockBottomDieline(parameters),
            BoxType.PillowBox => GeneratePillowBoxDieline(parameters),
            BoxType.RigidBox => GenerateRigidBoxDieline(parameters),
            _ => throw new NotImplementedException($"盒型 {parameters.Type} 的刀版生成尚未实现")
        };
    }

    /// <summary>
    /// 生成插口盒刀版
    /// 修正：添加顶部和底部的侧面防尘翼板
    /// </summary>
    private DielineData GenerateTuckEndDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float flapDepth = H * 0.6f;
        float dustFlapDepth = flapDepth * 0.5f; // 侧面防尘翼板深度
        float tabWidth = 15f;

        float currentX = 0;
        float currentY = dustFlapDepth; // 为底部防尘翼板留空间

        // 底部防尘翼板（左右侧面）
        var bottomDustLeft = CreateRectPanel("BottomDustLeft", PanelType.Flap,
            currentX, 0, W, dustFlapDepth);
        dieline.Panels.Add(bottomDustLeft);

        var bottomDustRight = CreateRectPanel("BottomDustRight", PanelType.Flap,
            currentX + W + L, 0, W, dustFlapDepth);
        dieline.Panels.Add(bottomDustRight);

        // 底部主翻盖（正面/背面）
        var bottomFlapFront = CreateRectPanel("BottomFlapFront", PanelType.Flap,
            currentX + W, 0, L, flapDepth);
        dieline.Panels.Add(bottomFlapFront);

        // 主体部分
        float bodyY = currentY;

        var leftPanel = CreateRectPanel("Left", PanelType.Left,
            currentX, bodyY, W, H);
        dieline.Panels.Add(leftPanel);

        var frontPanel = CreateRectPanel("Front", PanelType.Front,
            currentX + W, bodyY, L, H);
        dieline.Panels.Add(frontPanel);

        var rightPanel = CreateRectPanel("Right", PanelType.Right,
            currentX + W + L, bodyY, W, H);
        dieline.Panels.Add(rightPanel);

        var backPanel = CreateRectPanel("Back", PanelType.Back,
            currentX + W + L + W, bodyY, L, H);
        dieline.Panels.Add(backPanel);

        var glueTab = CreateRectPanel("GlueTab", PanelType.Glue,
            currentX + W + L + W + L, bodyY, tabWidth, H);
        dieline.Panels.Add(glueTab);

        currentY += H;

        // 顶部防尘翼板（左右侧面）
        var topDustLeft = CreateRectPanel("TopDustLeft", PanelType.Flap,
            currentX, currentY, W, dustFlapDepth);
        dieline.Panels.Add(topDustLeft);

        var topDustRight = CreateRectPanel("TopDustRight", PanelType.Flap,
            currentX + W + L, currentY, W, dustFlapDepth);
        dieline.Panels.Add(topDustRight);

        // 顶部主翻盖（正面）
        var topFlapFront = CreateRectPanel("TopFlapFront", PanelType.Flap,
            currentX + W, currentY, L, flapDepth);
        dieline.Panels.Add(topFlapFront);

        GenerateLinesFromPanels(dieline);
        dieline.Bounds = CalculateBounds(dieline);

        return dieline;
    }

    /// <summary>
    /// 生成邮寄盒刀版（飞机盒）
    /// 自锁翼板深度 = W/2，两侧翼板在底面中心相遇互锁
    /// </summary>
    private DielineData GenerateMailerDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float flapDepth = W / 2f; // 翼板深度必须为W/2，两侧翼板在底面中心相遇

        float centerX = H;
        float centerY = flapDepth;

        // 左侧面
        var leftPanel = CreateRectPanel("Left", PanelType.Left, 0, centerY, H, W);
        dieline.Panels.Add(leftPanel);

        // 底面（中心）
        var bottomPanel = CreateRectPanel("Bottom", PanelType.Bottom, centerX, centerY, L, W);
        dieline.Panels.Add(bottomPanel);

        // 右侧面
        var rightPanel = CreateRectPanel("Right", PanelType.Right, centerX + L, centerY, H, W);
        dieline.Panels.Add(rightPanel);

        // 顶部自锁翼板（前后各一个，深度=W/2）
        var topFlapFront = CreateRectPanel("TopFlapFront", PanelType.Flap,
            centerX, 0, L, flapDepth);
        dieline.Panels.Add(topFlapFront);

        var topFlapBack = CreateRectPanel("TopFlapBack", PanelType.Flap,
            centerX, centerY + W, L, flapDepth);
        dieline.Panels.Add(topFlapBack);

        // 左右侧面的顶部翼板
        var topFlapLeft = CreateRectPanel("TopFlapLeft", PanelType.Flap,
            0, 0, H, flapDepth);
        dieline.Panels.Add(topFlapLeft);

        var topFlapRight = CreateRectPanel("TopFlapRight", PanelType.Flap,
            centerX + L, 0, H, flapDepth);
        dieline.Panels.Add(topFlapRight);

        GenerateLinesFromPanels(dieline);
        dieline.Bounds = CalculateBounds(dieline);

        return dieline;
    }

    /// <summary>
    /// 生成瓦楞RSC刀版（标准开槽纸箱）
    /// 翼板深度 = W/2，两侧翼板在箱体中心相遇
    /// </summary>
    private DielineData GenerateCorrugatedRSCDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float flapLength = W / 2f; // 翼板深度必须为W/2，两侧翼板在箱体中心相遇

        // RSC布局：一字形展开
        // 左侧面 - 正面 - 右侧面 - 背面
        // 顶部和底部各有4个翼板

        float currentX = 0;
        float currentY = flapLength;

        // 底部翼板
        var bottomFlapLeft = CreateRectPanel("BottomFlapLeft", PanelType.Flap,
            currentX, 0, W, flapLength);
        dieline.Panels.Add(bottomFlapLeft);

        var bottomFlapFront = CreateRectPanel("BottomFlapFront", PanelType.Flap,
            currentX + W, 0, L, flapLength);
        dieline.Panels.Add(bottomFlapFront);

        var bottomFlapRight = CreateRectPanel("BottomFlapRight", PanelType.Flap,
            currentX + W + L, 0, W, flapLength);
        dieline.Panels.Add(bottomFlapRight);

        var bottomFlapBack = CreateRectPanel("BottomFlapBack", PanelType.Flap,
            currentX + W + L + W, 0, L, flapLength);
        dieline.Panels.Add(bottomFlapBack);

        // 主体侧面
        var leftPanel = CreateRectPanel("Left", PanelType.Left,
            currentX, currentY, W, H);
        dieline.Panels.Add(leftPanel);

        var frontPanel = CreateRectPanel("Front", PanelType.Front,
            currentX + W, currentY, L, H);
        dieline.Panels.Add(frontPanel);

        var rightPanel = CreateRectPanel("Right", PanelType.Right,
            currentX + W + L, currentY, W, H);
        dieline.Panels.Add(rightPanel);

        var backPanel = CreateRectPanel("Back", PanelType.Back,
            currentX + W + L + W, currentY, L, H);
        dieline.Panels.Add(backPanel);

        currentY += H;

        // 顶部翼板
        var topFlapLeft = CreateRectPanel("TopFlapLeft", PanelType.Flap,
            currentX, currentY, W, flapLength);
        dieline.Panels.Add(topFlapLeft);

        var topFlapFront = CreateRectPanel("TopFlapFront", PanelType.Flap,
            currentX + W, currentY, L, flapLength);
        dieline.Panels.Add(topFlapFront);

        var topFlapRight = CreateRectPanel("TopFlapRight", PanelType.Flap,
            currentX + W + L, currentY, W, flapLength);
        dieline.Panels.Add(topFlapRight);

        var topFlapBack = CreateRectPanel("TopFlapBack", PanelType.Flap,
            currentX + W + L + W, currentY, L, flapLength);
        dieline.Panels.Add(topFlapBack);

        GenerateLinesFromPanels(dieline);
        dieline.Bounds = CalculateBounds(dieline);

        return dieline;
    }

    /// <summary>
    /// 生成自动锁底盒刀版
    /// 锁扣翼板深度 = L/2
    /// </summary>
    private DielineData GenerateAutoLockBottomDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float flapDepth = H * 0.6f;
        float lockWingDepth = L * 0.5f; // 锁扣翼板深度 = L/2

        // 布局：一字形展开，底部有4个自动锁扣翼板
        float currentX = 0;
        float currentY = lockWingDepth;

        // 底部自动锁扣翼板（4个）
        var bottomWingLeft = CreateRectPanel("BottomWingLeft", PanelType.Flap,
            currentX, 0, W, lockWingDepth);
        dieline.Panels.Add(bottomWingLeft);

        var bottomWingFront = CreateRectPanel("BottomWingFront", PanelType.Flap,
            currentX + W, 0, L, lockWingDepth);
        dieline.Panels.Add(bottomWingFront);

        var bottomWingRight = CreateRectPanel("BottomWingRight", PanelType.Flap,
            currentX + W + L, 0, W, lockWingDepth);
        dieline.Panels.Add(bottomWingRight);

        var bottomWingBack = CreateRectPanel("BottomWingBack", PanelType.Flap,
            currentX + W + L + W, 0, L, lockWingDepth);
        dieline.Panels.Add(bottomWingBack);

        // 主体侧面
        var leftPanel = CreateRectPanel("Left", PanelType.Left,
            currentX, currentY, W, H);
        dieline.Panels.Add(leftPanel);

        var frontPanel = CreateRectPanel("Front", PanelType.Front,
            currentX + W, currentY, L, H);
        dieline.Panels.Add(frontPanel);

        var rightPanel = CreateRectPanel("Right", PanelType.Right,
            currentX + W + L, currentY, W, H);
        dieline.Panels.Add(rightPanel);

        var backPanel = CreateRectPanel("Back", PanelType.Back,
            currentX + W + L + W, currentY, L, H);
        dieline.Panels.Add(backPanel);

        currentY += H;

        // 顶部翻盖
        var topFlapFront = CreateRectPanel("TopFlapFront", PanelType.Flap,
            currentX + W, currentY, L, flapDepth);
        dieline.Panels.Add(topFlapFront);

        GenerateLinesFromPanels(dieline);
        dieline.Bounds = CalculateBounds(dieline);

        return dieline;
    }

    /// <summary>
    /// 生成枕头盒刀版（带弧形两端）
    /// 枕头盒展开为一张纸：主体矩形 + 两端弧形封口
    /// 弧形用多段折线近似（8段）
    /// </summary>
    private DielineData GeneratePillowBoxDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float curveDepth = W * 0.35f; // 弧形向内凹陷深度

        // 枕头盒展开：左弧 + 主体 + 右弧
        // 主体宽度 = L（盒长），高度 = H（盒高）
        // 两端弧形封口宽度 = curveDepth

        float bodyX = curveDepth;
        float bodyY = 0;

        // 主体面板（矩形）
        var bodyPanel = CreateRectPanel("Body", PanelType.Front, bodyX, bodyY, L, H);
        dieline.Panels.Add(bodyPanel);

        // 左端弧形封口（用多段折线近似椭圆弧）
        AddArcClosure(dieline, "LeftClosure",
            bodyX, bodyY, curveDepth, H, isLeft: true);

        // 右端弧形封口
        AddArcClosure(dieline, "RightClosure",
            bodyX + L, bodyY, curveDepth, H, isLeft: false);

        GenerateLinesFromPanels(dieline);

        // 弧形切割线（覆盖矩形近似的外边）
        AddArcCutLines(dieline, bodyX, bodyY, curveDepth, H, isLeft: true);
        AddArcCutLines(dieline, bodyX + L, bodyY, curveDepth, H, isLeft: false);

        dieline.Bounds = CalculateBounds(dieline);
        return dieline;
    }

    /// <summary>
    /// 添加弧形封口面板（多边形近似）
    /// </summary>
    private void AddArcClosure(DielineData dieline, string name,
        float attachX, float attachY, float depth, float height, bool isLeft)
    {
        int segments = 8;
        var points = new List<Vector2>();

        // 连接主体的两个角点
        if (isLeft)
        {
            points.Add(new Vector2(attachX, attachY));
            points.Add(new Vector2(attachX, attachY + height));
        }
        else
        {
            points.Add(new Vector2(attachX, attachY + height));
            points.Add(new Vector2(attachX, attachY));
        }

        // 弧形顶点（椭圆弧近似）
        float cx = isLeft ? attachX - depth : attachX + depth;
        float cy = attachY + height / 2f;
        float rx = depth;
        float ry = height / 2f;

        float startAngle = isLeft ? MathF.PI / 2f : -MathF.PI / 2f;
        float endAngle = isLeft ? -MathF.PI / 2f : MathF.PI / 2f;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = startAngle + t * (endAngle - startAngle);
            float x = cx + rx * MathF.Cos(angle);
            float y = cy + ry * MathF.Sin(angle);
            points.Add(new Vector2(x, y));
        }

        var panel = new Panel(name, PanelType.Flap, new Polygon2D(points))
        {
            WidthMM = depth,
            HeightMM = height
        };
        dieline.Panels.Add(panel);
    }

    /// <summary>
    /// 添加弧形切割线（替换矩形近似的直线外边）
    /// </summary>
    private void AddArcCutLines(DielineData dieline, float attachX, float attachY,
        float depth, float height, bool isLeft)
    {
        int segments = 16;
        float cx = isLeft ? attachX - depth : attachX + depth;
        float cy = attachY + height / 2f;
        float rx = depth;
        float ry = height / 2f;

        float startAngle = isLeft ? MathF.PI / 2f : -MathF.PI / 2f;
        float endAngle = isLeft ? -MathF.PI / 2f : MathF.PI / 2f;

        for (int i = 0; i < segments; i++)
        {
            float t1 = (float)i / segments;
            float t2 = (float)(i + 1) / segments;
            float a1 = startAngle + t1 * (endAngle - startAngle);
            float a2 = startAngle + t2 * (endAngle - startAngle);

            var p1 = new Vector2(cx + rx * MathF.Cos(a1), cy + ry * MathF.Sin(a1));
            var p2 = new Vector2(cx + rx * MathF.Cos(a2), cy + ry * MathF.Sin(a2));

            dieline.CutLines.Add(new Line2D(p1, p2, LineType.Cut));
        }
    }

    /// <summary>
    /// 生成天地盖精装盒刀版（两件式：盒盖+盒底）
    /// 修正：侧面高度应为各自的盖/底高度，而非W
    /// </summary>
    private DielineData GenerateRigidBoxDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float lidHeight = H * 0.4f;
        float baseHeight = H * 0.6f;
        float gap = 20f;

        // 盒底部分（左侧）
        float baseX = 0;
        float baseY = 0;

        var baseBottom = CreateRectPanel("BaseBottom", PanelType.Bottom,
            baseX + W, baseY + baseHeight, L, W);
        dieline.Panels.Add(baseBottom);

        var baseFront = CreateRectPanel("BaseFront", PanelType.Front,
            baseX + W, baseY, L, baseHeight);
        dieline.Panels.Add(baseFront);

        var baseBack = CreateRectPanel("BaseBack", PanelType.Back,
            baseX + W, baseY + baseHeight + W, L, baseHeight);
        dieline.Panels.Add(baseBack);

        // 修正：侧面高度用 baseHeight，不是 W
        var baseLeft = CreateRectPanel("BaseLeft", PanelType.Left,
            baseX, baseY + baseHeight, W, baseHeight);
        dieline.Panels.Add(baseLeft);

        var baseRight = CreateRectPanel("BaseRight", PanelType.Right,
            baseX + W + L, baseY + baseHeight, W, baseHeight);
        dieline.Panels.Add(baseRight);

        // 盒盖部分（右侧，盖内径略大于底外径）
        float lidX = baseX + W + L + W + gap;
        float lidY = 0;

        var lidTop = CreateRectPanel("LidTop", PanelType.Bottom,
            lidX + W, lidY + lidHeight, L, W);
        dieline.Panels.Add(lidTop);

        var lidFront = CreateRectPanel("LidFront", PanelType.Front,
            lidX + W, lidY, L, lidHeight);
        dieline.Panels.Add(lidFront);

        var lidBack = CreateRectPanel("LidBack", PanelType.Back,
            lidX + W, lidY + lidHeight + W, L, lidHeight);
        dieline.Panels.Add(lidBack);

        // 修正：侧面高度用 lidHeight，不是 W
        var lidLeft = CreateRectPanel("LidLeft", PanelType.Left,
            lidX, lidY + lidHeight, W, lidHeight);
        dieline.Panels.Add(lidLeft);

        var lidRight = CreateRectPanel("LidRight", PanelType.Right,
            lidX + W + L, lidY + lidHeight, W, lidHeight);
        dieline.Panels.Add(lidRight);

        GenerateLinesFromPanels(dieline);
        dieline.Bounds = CalculateBounds(dieline);

        return dieline;
    }

    /// <summary>
    /// 创建矩形面板
    /// </summary>
    private Panel CreateRectPanel(string name, PanelType type, float x, float y, float width, float height)
    {
        var points = new List<Vector2>
        {
            new Vector2(x, y),
            new Vector2(x + width, y),
            new Vector2(x + width, y + height),
            new Vector2(x, y + height)
        };

        var polygon = new Polygon2D(points);
        var panel = new Panel(name, type, polygon)
        {
            WidthMM = width,
            HeightMM = height
        };

        return panel;
    }

    /// <summary>
    /// 从面板生成切割线和折叠线
    /// </summary>
    private void GenerateLinesFromPanels(DielineData dieline)
    {
        // 收集所有边
        var allEdges = new List<(Vector2 start, Vector2 end, Panel panel)>();

        foreach (var panel in dieline.Panels)
        {
            var points = panel.Boundary.Points;
            for (int i = 0; i < points.Count; i++)
            {
                int j = (i + 1) % points.Count;
                allEdges.Add((points[i], points[j], panel));
            }
        }

        // 检测共享边（折叠线）和外边（切割线）
        var processedEdges = new HashSet<string>();

        foreach (var edge in allEdges)
        {
            string edgeKey = GetEdgeKey(edge.start, edge.end);
            if (processedEdges.Contains(edgeKey))
                continue;

            // 查找是否有其他面板共享这条边
            bool isShared = allEdges.Any(e =>
                e.panel != edge.panel &&
                EdgesMatch(edge.start, edge.end, e.start, e.end));

            if (isShared)
            {
                // 共享边 = 折叠线
                dieline.FoldLines.Add(new Line2D(edge.start, edge.end, LineType.Fold));
            }
            else
            {
                // 外边 = 切割线
                dieline.CutLines.Add(new Line2D(edge.start, edge.end, LineType.Cut));
            }

            processedEdges.Add(edgeKey);
        }
    }

    private string GetEdgeKey(Vector2 p1, Vector2 p2)
    {
        // 标准化边的表示（确保顺序一致）
        if (p1.X < p2.X || (p1.X == p2.X && p1.Y < p2.Y))
            return $"{p1.X},{p1.Y}-{p2.X},{p2.Y}";
        else
            return $"{p2.X},{p2.Y}-{p1.X},{p1.Y}";
    }

    private bool EdgesMatch(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        const float tolerance = 0.01f;
        return (Vector2.Distance(a1, b1) < tolerance && Vector2.Distance(a2, b2) < tolerance) ||
               (Vector2.Distance(a1, b2) < tolerance && Vector2.Distance(a2, b1) < tolerance);
    }

    private BoundingBox2D CalculateBounds(DielineData dieline)
    {
        var allPoints = new List<Vector2>();
        foreach (var panel in dieline.Panels)
        {
            allPoints.AddRange(panel.Boundary.Points);
        }
        return BoundingBox2D.FromPoints(allPoints);
    }
}
