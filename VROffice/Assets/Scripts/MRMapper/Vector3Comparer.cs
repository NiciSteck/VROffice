using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Comparer : IComparer<Vector3>
{
    public int Compare(Vector3 a, Vector3 b)
    {
        // Compare x component
        if (a.x < b.x)
            return -1;
        else if (a.x > b.x)
            return 1;

        // Compare y component
        if (a.y < b.y)
            return -1;
        else if (a.y > b.y)
            return 1;

        // Compare z component
        if (a.z < b.z)
            return -1;
        else if (a.z > b.z)
            return 1;

        return 0; // Vectors are equal
    }
}