using System;
using UnityEngine;

public static class Vector3Extension
{
    public static Vector3 SetComponent(this Vector3 vector, Axis axis, float value)
    {
        switch (axis) 
        {
            case Axis.X:
                vector.x = value;

                return vector;
            case Axis.Y:
                vector.y = value;

                return vector;
            case Axis.Z:
                vector.z = value;

                return vector;
            default:
                throw new NotImplementedException(); 
        }
    }

    public static float GetComponent(this Vector3 vector, Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return vector.x;
            case Axis.Y:
                return vector.y;
            case Axis.Z:
                return vector.z;
            default:
                throw new NotImplementedException();
        }
    }
}
