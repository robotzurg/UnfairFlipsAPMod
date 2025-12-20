// Decompiled with JetBrains decompiler
// Type: Mathy
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CBDAB4D2-9C6B-4E2B-BD9C-23B148109529
// Assembly location: C:\Users\admin\RiderProjects\UnfairFlipsAPMod\obj\Debug\PublicizedAssemblies\Assembly-CSharp.93B0C47EF44010426CA366C400DF0B88\Assembly-CSharp.dll

using System;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#nullable disable
public static class Mathy
{
    public static float Decay(float a, float b, float decay, float dt)
    {
        return b + (a - b) * Mathf.Exp(-decay * dt);
    }

    public static Vector3 Decay(Vector3 a, Vector3 b, float decay, float dt)
    {
        return b + (a - b) * Mathf.Exp(-decay * dt);
    }

    public static Vector2 Decay(Vector2 a, Vector2 b, float decay, float dt)
    {
        return b + (a - b) * Mathf.Exp(-decay * dt);
    }

    public static Color Decay(Color a, Color b, float decay, float dt)
    {
        return b + (a - b) * Mathf.Exp(-decay * dt);
    }

    public static float AngleBetween(Vector2 vector1, Vector2 vector2)
    {
        return Mathf.Atan2((float) ((double) vector1.x * (double) vector2.y - (double) vector2.x * (double) vector1.y), (float) ((double) vector1.x * (double) vector2.x + (double) vector1.y * (double) vector2.y)) * 57.295776f;
    }

    public static string CentsToDollarString(BigInteger cents)
    {
        BigInteger num = cents / 100L;
        string str1 = num.ToString();
        num = cents % 100L;
        string str2 = num.ToString("D2");
        return $"${str1}.{str2}";
    }

    public static string CentsToDollarString(int cents)
    {
        int num = cents / 100;
        string str1 = num.ToString();
        num = cents % 100;
        string str2 = num.ToString("D2");
        return $"${str1}.{str2}";
    }
}