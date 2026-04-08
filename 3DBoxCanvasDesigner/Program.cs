using PicoGK;

namespace BoxCanvasDesigner;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Library.Go(0.5f, Example.Run);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

class Example
{
    public static void Run()
    {
        try
        {
            // 创建盒型参数 - 邮寄盒示例
            // 内部尺寸: 220mm × 150mm × 80mm, 壁厚: 3mm
            var parameters = new BoxParameters(
                Type: BoxType.Mailer,
                LengthMM: 220,
                WidthMM: 150,
                HeightMM: 80,
                WallThicknessMM: 3.0f
            );

            // 生成并预览盒子
            var generator = new BoxGenerator(parameters);
            generator.Preview();

            Console.WriteLine("3D Box Canvas Designer - 参数化盒型生成成功");
        }
        catch (Exception ex)
        {
            Library.Log($"运行错误: {ex.Message}");
        }
    }
}
