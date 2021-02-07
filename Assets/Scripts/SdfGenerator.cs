using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class SdfGenerator
{
    private static ComputeBuffer GetOutputBuffer(int sdfResolution)
    {
        int cubicSdfResolution = sdfResolution * sdfResolution * sdfResolution;
        ComputeBuffer outputBuffer = new ComputeBuffer(cubicSdfResolution, Marshal.SizeOf(typeof(float)));
        return outputBuffer;
    }

    private static ComputeBuffer GetMeshNormalsBuffer(Mesh mesh)
    {
        ComputeBuffer outputBuffer = new ComputeBuffer(mesh.normals.Length, Marshal.SizeOf(typeof(Vector3)));
        outputBuffer.SetData(mesh.normals);
        return outputBuffer;
    }

    private static ComputeBuffer GetMeshVerticesBuffer(Mesh mesh)
    {
        ComputeBuffer outputBuffer = new ComputeBuffer(mesh.vertices.Length, Marshal.SizeOf(typeof(Vector3)));
        outputBuffer.SetData(mesh.vertices);
        return outputBuffer;
    }

    private static ComputeBuffer GetMeshTrianglesBuffer(Mesh mesh)
    {
        ComputeBuffer outputBuffer = new ComputeBuffer(mesh.triangles.Length, Marshal.SizeOf(typeof(int)));
        outputBuffer.SetData(mesh.triangles);
        return outputBuffer;
    }

    private static Vector3 ComputeMinExtents(Bounds meshBounds)
    {
        float largestSide = MaxComponent(meshBounds.size);
        float padding = largestSide / 20;
        return meshBounds.center - (Vector3.one * (largestSide * 0.5f + padding));
    }

    private static Vector3 ComputeMaxExtents(Bounds meshBounds)
    {
        float largestSide = MaxComponent(meshBounds.size);
        float padding = largestSide / 20;
        return meshBounds.center + (Vector3.one * (largestSide * 0.5f + padding));
    }

    private static float MaxComponent(Vector3 vector)
    {
        return Mathf.Max(vector.x, vector.y, vector.z);
    }

    private static Texture3D Texture3dFromData(float[] outputData, int sdfResolution)
    {
        Texture3D tex3D = new Texture3D(sdfResolution, sdfResolution, sdfResolution, TextureFormat.RFloat, false);
        tex3D.anisoLevel = 1;
        tex3D.filterMode = FilterMode.Bilinear;
        tex3D.wrapMode = TextureWrapMode.Clamp;

        Color[] colors = tex3D.GetPixels();
        for (int y = 0; y < sdfResolution; y++)
        {
            for (int z = 0; z < sdfResolution; z++)
            {
                for (int x = 0; x < sdfResolution; x++)
                {
                    int index = x + y * sdfResolution + z * sdfResolution * sdfResolution;
                    float c = outputData[index];
                    colors[index] = new Color(c, 0, 0, 0);
                }
            }
        }

        tex3D.SetPixels(colors);
        tex3D.Apply(false, false);
        return tex3D;
    }

    public static Texture3D GenerateSdf(Mesh mesh, int sdfResolution)
    {
        // let's start with some safety checks
        if (mesh == null)
        {
            Debug.LogError("The mesh provided to the SDF Generator must not be null.");
        }
        if (sdfResolution % 8 != 0) // check if the sdf resolution is a power of 8
        {
            Debug.LogError("Sdf resolution must be a power of 8.");
        }

        // create a timer
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        double computeTime;
        double copyTime;

        // create the resources we need to upload and retrive data from the GPU
        ComputeBuffer outputBuffer = GetOutputBuffer(sdfResolution);
        ComputeBuffer meshTrianglesBuffer = GetMeshTrianglesBuffer(mesh);
        ComputeBuffer meshVerticesBuffer = GetMeshVerticesBuffer(mesh);
        ComputeBuffer meshNormalsBuffer = GetMeshNormalsBuffer(mesh);

        // Instantiate the compute shader from the resources folder
        ComputeShader computeShader = (ComputeShader)Resources.Load<ComputeShader>("SDF_Generator");
        int kernel = computeShader.FindKernel("CSMain");

        // bind the resources to the compute shader
        computeShader.SetInt("SdfResolution", sdfResolution);
        computeShader.SetBuffer(kernel, "MeshTrianglesBuffer", meshTrianglesBuffer);
        computeShader.SetBuffer(kernel, "MeshVerticesBuffer", meshVerticesBuffer);
        computeShader.SetBuffer(kernel, "MeshNormalsBuffer", meshNormalsBuffer);
        computeShader.SetBuffer(kernel, "Output", outputBuffer);
        computeShader.SetVector("MinExtents", ComputeMinExtents(mesh.bounds));
        computeShader.SetVector("MaxExtents", ComputeMaxExtents(mesh.bounds));

        // dispatch
        int threadGroupSize = sdfResolution / 8;
        stopwatch.Start();
        computeShader.Dispatch(kernel, threadGroupSize, threadGroupSize, threadGroupSize);
        float[] outputData = new float[sdfResolution * sdfResolution * sdfResolution];
        outputBuffer.GetData(outputData);
        stopwatch.Stop();
        computeTime = stopwatch.Elapsed.TotalSeconds;

        stopwatch.Restart();
        // convert output data ComputeBuffer to a 3D texture
        Texture3D tex3d = Texture3dFromData(outputData, sdfResolution);
        stopwatch.Stop();
        copyTime = stopwatch.Elapsed.TotalSeconds;

        // destroy the resources
        outputBuffer.Release();
        meshTrianglesBuffer.Release();
        meshVerticesBuffer.Release();
        meshNormalsBuffer.Release();

        // print computational duration data
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"SDF Genearated in {(computeTime + copyTime).ToString("F4")} seconds");
        sb.AppendLine("↓↓ Click for more details ↓↓");
        sb.AppendLine($"=> Compute Time: {computeTime.ToString("F4")} seconds");
        sb.AppendLine($"=> Copy Time: {copyTime.ToString("F4")} seconds");
        Debug.Log(sb.ToString());

        return tex3d;
    }
}
