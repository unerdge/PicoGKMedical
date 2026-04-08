using PicoGK;
using System.Numerics;

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
            Library.oViewer().SetGroupMaterial(0, "FF0000", 0.0f, 1.0f);

            // 创建一个简单的盒子示例
            Lattice lat = new Lattice();
            lat.AddBeam(new Vector3(0, 0, 0), new Vector3(50, 0, 0), 2.0f, 2.0f, false);
            lat.AddBeam(new Vector3(50, 0, 0), new Vector3(50, 50, 0), 2.0f, 2.0f, false);
            lat.AddBeam(new Vector3(50, 50, 0), new Vector3(0, 50, 0), 2.0f, 2.0f, false);
            lat.AddBeam(new Vector3(0, 50, 0), new Vector3(0, 0, 0), 2.0f, 2.0f, false);

            Voxels voxBox = new Voxels(lat);
            Library.oViewer().Add(voxBox);

            Console.WriteLine("3D Box Canvas Designer - 示例运行成功");
        }
        catch (Exception ex)
        {
            Library.Log($"运行错误: {ex.Message}");
        }
    }
}
