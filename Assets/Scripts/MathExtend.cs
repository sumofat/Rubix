using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class MathExtend
{
   public static int[,] Transpose(int dimension, int[,] matrix)
    {
        int[,] transposed = new int[dimension, dimension];
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                transposed[i, j] = matrix[j, i];
            }
        }
        return transposed;
    }
    public static void Swap(ref int a, ref int b)
    {
        (b, a) = (a, b);
    }

    public static int[,] SwapColumns(int dimension,int[,] matrix)
    {
        //int[,] ColumnSwappec = new int[CubeDim, CubeDim];
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension / 2; j++)
            {
                Swap(ref matrix[i, j], ref matrix[i, dimension - j - 1]);
            }
        }
        return matrix;
    }

    public static int[,] SwapRows(int dimension,int[,] matrix)
    {
        //swap rows for counter clockwise
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension / 2; j++)
            {
                Swap(ref matrix[j, i], ref matrix[dimension - j - 1, i]);
            }
        }
        return matrix;
    }
}
