using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    private MeshRenderer meshRenderer;

    public bool drawOriginalBounds = true;
    public bool drawComputedBounds = true;

    void Start()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        Bounds bounds = meshRenderer.bounds;
        Debug.Log($"Center: {bounds.center.ToString("F4")} | Size: {bounds.size.ToString("F4")} | Extents: {bounds.extents.ToString("F4")} | Min: {bounds.min.ToString("F4")} | Max: {bounds.max.ToString("F4")}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private static float MaxComponent(Vector3 vector)
    {
        return Mathf.Max(vector.x, vector.y, vector.z);
    }


    private void OnDrawGizmosSelected()
    {
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();

        if (drawOriginalBounds)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(mf.sharedMesh.bounds.center, mf.sharedMesh.bounds.size);
        }


        if (drawComputedBounds)
        {
            float largestSide = MaxComponent(mf.sharedMesh.bounds.size);
            float padding = largestSide / 20;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(mf.sharedMesh.bounds.center, Vector3.one * (largestSide + padding * 2));
        }
    }
}
