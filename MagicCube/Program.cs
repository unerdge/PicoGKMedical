using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;


namespace TestHello
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Reflection.Emit;
    using System.Text.RegularExpressions;
    using static System.Math;
    class Program
    {
        static void Main(string[] args)
        {
            MagicCube dim3Cube=new MagicCube(3,MagicCube.ScanOrder.CenterFirst);
            dim3Cube.Search();
        }
    }
}

