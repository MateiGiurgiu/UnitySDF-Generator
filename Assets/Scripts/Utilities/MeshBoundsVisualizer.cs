using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBoundsVisualizer : MonoBehaviour
{
    public bool drawOriginalBounds = true;
    public bool drawComputedBounds = true;

    private static float MaxComponent(Vector3 vector)
    {
        return Mathf.Max(vector.x, vector.y, vector.z);
    }

    private void OnDrawGizmosSelected()
    {
        //MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

        if (drawOriginalBounds)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(mr.bounds.center, mr.bounds.size);
        }


        if (drawComputedBounds)
        {
            float largestSide = MaxComponent(mr.bounds.size);
            float padding = largestSide / 20;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(mr.bounds.center, Vector3.one * (largestSide + padding * 2));
        }
    }
}
