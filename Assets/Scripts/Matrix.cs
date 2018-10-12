using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Class used to do matrix-operations. 
//Added static methods to be able to multiply arrays of floats as if they were Matrices
public class Matrix
{

    public float[,] matrix;

    public Matrix(float[,] input)
    {
        matrix = input;
    }

    public Matrix(int rows, int columns, float[] input)
    {
        float[,] mat = new float[rows, columns];
        int count = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                mat[i, j] = input[count];
                count++;
            }
        }
        matrix = mat;
    }



    public static float[,] matMult(float[,] m1, float[,] m2) //Multiplies two matrices, outputs product.
    {
        int m1Rows = m1.GetLength(0);
        int m1Columns = m1.GetLength(1);
        int m2Rows = m2.GetLength(0);
        int m2Columns = m2.GetLength(1);

        if (m1Columns != m2Rows)
            Debug.Log("ROWS AND COLUMN MISMATCH");

        float[,] m3 = new float[m1.GetLength(0), m2.GetLength(1)];

        for (int i = 0; i < m1Rows; i++)
        {
            for (int j = 0; j < m2Columns; j++)
            {
                for (int k = 0; k < m1Columns; k++)
                {
                    m3[i, j] += m1[i, k] * m2[k, j];
                }
            }
        }


        return m3;
    }

    public float[,] MatAsArr()
    {
        return matrix;
    }

    public static Matrix multiply(Matrix m1, Matrix m2) //Multiplies two matrices, outputs Matrix.
    {
        {
            Matrix m3 = new Matrix(matMult(m1.matrix, m2.matrix));
            return m3;
        }
    }
}
