using System;
using System.Numerics;
using BreakInfinity;
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
    
    public static string CentsToDollarString(BigDouble totalCents)
    {
        var dollars = (totalCents / 100).Floor();
        var cents = (totalCents - (dollars * 100)).Round();
        return dollars < 10000 ? 
            $"${dollars.ToString("F0")}.{cents.ToString("F0").PadLeft(2, '0')}" 
            : $"${dollars.ToString("G4")}";
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