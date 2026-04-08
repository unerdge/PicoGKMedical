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
    /// </summary>
    private DielineData GenerateTuckEndDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float flapDepth = H * 0.6f; // 翻盖深度默认为高度的60%
        float tabWidth = 15f;       // 插舌宽度

        // 布局：从左到右依次为 左侧面 - 正面 - 右侧面 - 背面 - 粘合边
        // 从下到上：底部翻盖 - 主体 - 顶部翻盖

        float currentX = 0;
        float currentY = 0;

        // 底部翻盖（正面）
        var bottomFlapFront = CreateRectPanel("BottomFlapFront", PanelType.Flap,
            currentX + W, currentY, L, flapDepth);
        dieline.Panels.Add(bottomFlapFront);
        currentY += flapDepth;

        // 主体部分
        float bodyY = currentY;

        // 左侧面
        var leftPanel = CreateRectPanel("Left", PanelType.Left,
            currentX, bodyY, W, H);
        dieline.Panels.Add(leftPanel);

        // 正面
        var frontPanel = CreateRectPanel("Front", PanelType.Front,
            currentX + W, bodyY, L, H);
        dieline.Panels.Add(frontPanel);

        // 右侧面
        var rightPanel = CreateRectPanel("Right", PanelType.Right,
            currentX + W + L, bodyY, W, H);
        dieline.Panels.Add(rightPanel);

        // 背面
        var backPanel = CreateRectPanel("Back", PanelType.Back,
            currentX + W + L + W, bodyY, L, H);
        dieline.Panels.Add(backPanel);

        // 粘合边
        var glueTab = CreateRectPanel("GlueTab", PanelType.Glue,
            currentX + W + L + W + L, bodyY, tabWidth, H);
        dieline.Panels.Add(glueTab);

        currentY += H;

        // 顶部翻盖（正面）
        var topFlapFront = CreateRectPanel("TopFlapFront", PanelType.Flap,
            currentX + W, currentY, L, flapDepth);
        dieline.Panels.Add(topFlapFront);

        // 生成切割线和折叠线
        GenerateLinesFromPanels(dieline);

        // 计算边界框
        dieline.Bounds = CalculateBounds(dieline);

        return dieline;
    }

    /// <summary>
    /// 生成邮寄盒刀版（飞机盒）
    /// </summary>
    private DielineData GenerateMailerDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;

        // 邮寄盒布局：十字形展开
        // 中心是底面，四周是四个侧面，侧面顶部有自锁翼板

        float centerX = W;
        float centerY = L;

        // 底面（中心）
        var bottomPanel = CreateRectPanel("Bottom", PanelType.Bottom,
            centerX, centerY, L, W);
        dieline.Panels.Add(bottomPanel);

        // 前侧面
        var frontPanel = CreateRectPanel("Front", PanelType.Front,
            centerX, centerY - H, L, H);
        dieline.Panels.Add(frontPanel);

        // 后侧面
        var backPanel = CreateRectPanel("Back", PanelType.Back,
            centerX, centerY + W, L, H);
        dieline.Panels.Add(backPanel);

        // 左侧面
        var leftPanel = CreateRectPanel("Left", PanelType.Left,
            centerX - H, centerY, H, W);
        dieline.Panels.Add(leftPanel);

        // 右侧面
        var rightPanel = CreateRectPanel("Right", PanelType.Right,
            centerX + L, centerY, H, W);
        dieline.Panels.Add(rightPanel);

        // 顶部自锁翼板（4个）
        float flapDepth = H * 0.5f;

        var topFlapFront = CreateRectPanel("TopFlapFront", PanelType.Flap,
            centerX, centerY - H - flapDepth, L, flapDepth);
        dieline.Panels.Add(topFlapFront);

        var topFlapBack = CreateRectPanel("TopFlapBack", PanelType.Flap,
            centerX, centerY + W + H, L, flapDepth);
        dieline.Panels.Add(topFlapBack);

        var topFlapLeft = CreateRectPanel("TopFlapLeft", PanelType.Flap,
            centerX - H - flapDepth, centerY, flapDepth, W);
        dieline.Panels.Add(topFlapLeft);

        var topFlapRight = CreateRectPanel("TopFlapRight", PanelType.Flap,
            centerX + L + H, centerY, flapDepth, W);
        dieline.Panels.Add(topFlapRight);

        GenerateLinesFromPanels(dieline);
        dieline.Bounds = CalculateBounds(dieline);

        return dieline;
    }

    /// <summary>
    /// 生成瓦楞RSC刀版（标准开槽纸箱）
    /// </summary>
    private DielineData GenerateCorrugatedRSCDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float flapLength = L / 2f; // 翼板长度为长度的一半

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
    /// </summary>
    private DielineData GenerateAutoLockBottomDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float flapDepth = H * 0.6f;
        float lockWingDepth = W * 0.5f; // 锁扣翼板深度

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
    /// 生成枕头盒刀版（简化版：矩形面板，实际应有弧形边）
    /// </summary>
    private DielineData GeneratePillowBoxDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float curveDepth = W * 0.3f; // 弧形深度

        // 枕头盒布局：中心主体 + 两端弧形封口
        float currentX = curveDepth;
        float currentY = 0;

        // 左端弧形封口（简化为矩形）
        var leftCurve = CreateRectPanel("LeftCurve", PanelType.Flap,
            0, currentY, curveDepth, H);
        dieline.Panels.Add(leftCurve);

        // 主体面板
        var bodyPanel = CreateRectPanel("Body", PanelType.Front,
            currentX, currentY, L, H);
        dieline.Panels.Add(bodyPanel);

        // 右端弧形封口（简化为矩形）
        var rightCurve = CreateRectPanel("RightCurve", PanelType.Flap,
            currentX + L, currentY, curveDepth, H);
        dieline.Panels.Add(rightCurve);

        GenerateLinesFromPanels(dieline);
        dieline.Bounds = CalculateBounds(dieline);

        return dieline;
    }

    /// <summary>
    /// 生成天地盖精装盒刀版（两件式：盒盖+盒底）
    /// </summary>
    private DielineData GenerateRigidBoxDieline(BoxParameters p)
    {
        var dieline = new DielineData();
        float L = p.LengthMM;
        float W = p.WidthMM;
        float H = p.HeightMM;
        float lidHeight = H * 0.4f;  // 盒盖高度
        float baseHeight = H * 0.6f; // 盒底高度
        float gap = 20f; // 盒盖和盒底之间的间距

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

        var baseLeft = CreateRectPanel("BaseLeft", PanelType.Left,
            baseX, baseY + baseHeight, W, W);
        dieline.Panels.Add(baseLeft);

        var baseRight = CreateRectPanel("BaseRight", PanelType.Right,
            baseX + W + L, baseY + baseHeight, W, W);
        dieline.Panels.Add(baseRight);

        // 盒盖部分（右侧）
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

        var lidLeft = CreateRectPanel("LidLeft", PanelType.Left,
            lidX, lidY + lidHeight, W, W);
        dieline.Panels.Add(lidLeft);

        var lidRight = CreateRectPanel("LidRight", PanelType.Right,
            lidX + W + L, lidY + lidHeight, W, W);
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
