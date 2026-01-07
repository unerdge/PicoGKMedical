using System;
using System.Numerics;

namespace MatrixLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            Matrix4x4 a = new Matrix4x4(
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 10, 11, 12,
                13, 14, 15, 16
            );
            // Create a 2D array (matrix) with 3 rows and 4 columns
            int[,] matrix = new int[3, 4]
            {
                { 1, 2, 3, 4 },
                { 5, 6, 7, 8 },
                { 9, 10, 11, 12 }
            };

            // Print the matrix to the console
            Console.WriteLine("Matrix:");
            for (int i = 0; i < matrix.GetLength(0); i++) // Iterate through rows
            {
                for (int j = 0; j < matrix.GetLength(1); j++) // Iterate through columns
                {
                    Console.Write(matrix[i, j] + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}