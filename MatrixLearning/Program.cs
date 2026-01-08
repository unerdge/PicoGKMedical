using System;
using System.Numerics;

namespace MatrixLearning
{
    class Program
    {
        static void PrintMatrix(Matrix4x4 matrix)
        {
            Console.WriteLine($"{matrix.M11}\t{matrix.M12}\t{matrix.M13}\t{matrix.M14}");
            Console.WriteLine($"{matrix.M21}\t{matrix.M22}\t{matrix.M23}\t{matrix.M24}");
            Console.WriteLine($"{matrix.M31}\t{matrix.M32}\t{matrix.M33}\t{matrix.M34}");
            Console.WriteLine($"{matrix.M41}\t{matrix.M42}\t{matrix.M43}\t{matrix.M44}");
        }
        static void Main(string[] args)
        {
            Matrix4x4 a = new Matrix4x4(
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 10, 11, 12,
                13, 14, 15, 16
            );
            //why not use var
            Matrix4x4 b = new Matrix4x4(
                16, 15, 14, 13,
                12, 11, 10, 9,
                8, 7, 6, 5,
                4, 3, 2, 1
            );
            Matrix4x4 result = Matrix4x4.Multiply(a, b);
            Console.WriteLine("Result of Matrix Multiplication:");
            PrintMatrix(result);
        }
    }
}