﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector {

    public static float[] Multiply(float[] v1, float[] v2)
    {
        float[] v3 = new float[v1.Length];
        for (int i = 0; i < v1.Length; i++)
            v3[i] = v1[i] * v2[i];
        return v3;
    }

    public static float[] matVecMult(float[] v, float[,] m)
    {
        float[] ret = new float[m.GetLength(1)];
        for (int i = 0; i < m.GetLength(1); i++)
        {
            for(int j = 0; j <v.GetLength(0); j++)
            {
                ret[i] += v[j] * m[j, i];
            }
        }
        return ret;
    }

    public static float[] Add(float[] v1, float[] v2)
    {
        float[] sum = new float[v1.Length]; 
        for (int i = 0; i < v1.Length; i++)
        {
            sum[i] = v1[i] + v2[i];
        }
        return sum;
    }
    public static float[] Subtract(float[] v1, float[] v2)
    {
        float[] sum = new float[v1.Length];
        for (int i = 0; i < v1.Length; i++)
        {
            sum[i] = v1[i] - v2[i];
        }
        return sum;
    }

}