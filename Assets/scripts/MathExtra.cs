using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public static class MathExtra 
{
    public static Vector3 ToXZ(this Vector2 input)
    {
        return new Vector3(input.x, 0.0f, input.y);
    }

    public static Vector3 ToXZ(this Vector3 input)
    {

        return new Vector3(input.x, 0.0f, input.z);
    }

    
    public static Vector3 ToZXMinus(this Vector2 input)
    {
        return new Vector3(-input.y, 0.0f,input.x);
    }

    public static Vector3 MoveTowards(Vector3 from , Vector3 to, float delta)
    {
        return from + ( ( to.magnitude >0) ? (to - from) * delta : Vector3.zero);

    }
    
    public static Vector3 WithZ(this Vector2 input, float z)
    {
        return new Vector3(input.x,input.y,z);

    }
   
}
