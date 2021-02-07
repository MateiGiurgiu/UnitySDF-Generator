using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SDFImageEffect : MonoBehaviour
{
    [SerializeField] private Shader sdfVisualiserShader = null;
    [SerializeField] private Texture3D sdfTexture = null;

    [SerializeField, Range(0.25f, 10f)] private float sdfScale = 1f;
    [SerializeField] private Vector3 sdfPosition = Vector3.zero;

    [Range(0.0f, 1.0f)] public float Depth = 0.0f;

    private Material _imageEffectMaterial = null;
    private Material imageEffectMaterial
    {
        get
        {
            if (!_imageEffectMaterial && sdfVisualiserShader)
            {
                _imageEffectMaterial = new Material(sdfVisualiserShader);
                _imageEffectMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return _imageEffectMaterial;
        }
    }

    private Camera _currentCamera = null;
    public Camera currentCamera
    {
        get
        {
            if (!_currentCamera)
            {
                _currentCamera = gameObject.GetComponent<Camera>();
            }
            return _currentCamera;
        }
    }

    
    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!imageEffectMaterial || !sdfTexture)
        {
            Graphics.Blit(source, destination);
        }
        else
        {
            // pass frustum rays to shader
            imageEffectMaterial.SetTexture("_SDF", sdfTexture);
            imageEffectMaterial.SetMatrix("_SDFMappingMatrix", GetVolumeMappingFunction());
            imageEffectMaterial.SetMatrix("_FrustumCornersMatrix", GetFrustumCorners());
            imageEffectMaterial.SetFloat("_SdfScale", sdfScale);
            imageEffectMaterial.SetMatrix("_CameraInvViewMatrix", currentCamera.cameraToWorldMatrix);
            imageEffectMaterial.SetFloat("_Depth", Depth);
            CustomGraphicsBlit(source, destination, imageEffectMaterial, 0);
        }
    }
    
    private void CustomGraphicsBlit(RenderTexture source, RenderTexture destination, Material effect, int pass)
    {
        RenderTexture.active = destination;

        effect.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();
        GL.LoadIdentity();

        effect.SetPass(pass);

        // make a quad
        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // Bottom Left

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // Bottom Right

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // Top Right

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // Top Left

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 GetVolumeMappingFunction()
    {
        Matrix4x4 scale = Matrix4x4.Scale(Vector3.one * (1f / sdfScale));
        Matrix4x4 translate = Matrix4x4.Translate(-sdfPosition + Vector3.one * (sdfScale / 2f));
        return scale * translate;
    }

    private Matrix4x4 GetFrustumCorners()
    {
        float camFov = currentCamera.fieldOfView;
        float camAspect = currentCamera.aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float halfFov = camFov * 0.5f;

        float tanFov = Mathf.Tan(halfFov * Mathf.Deg2Rad);

        Vector3 toRight = Vector3.right * tanFov * camAspect;
        Vector3 toTop = Vector3.up * tanFov;

        Vector3 topLeft = Vector3.Normalize(-Vector3.forward - toRight + toTop);
        Vector3 topRight = Vector3.Normalize(-Vector3.forward + toRight + toTop);
        Vector3 bottomRight = Vector3.Normalize(-Vector3.forward + toRight - toTop);
        Vector3 bottomLeft = Vector3.Normalize(-Vector3.forward - toRight - toTop);

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);

        return frustumCorners;
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 frustumCorners = GetFrustumCorners();

        Vector3 topLeft = currentCamera.cameraToWorldMatrix.MultiplyVector(frustumCorners.GetRow(0));
        Vector3 topRight = currentCamera.cameraToWorldMatrix.MultiplyVector(frustumCorners.GetRow(1));
        Vector3 bottomRight = currentCamera.cameraToWorldMatrix.MultiplyVector(frustumCorners.GetRow(2));
        Vector3 bottomLeft = currentCamera.cameraToWorldMatrix.MultiplyVector(frustumCorners.GetRow(3));

        Gizmos.color = Color.green;

        Gizmos.DrawRay(currentCamera.transform.position, topLeft);
        Gizmos.DrawRay(currentCamera.transform.position, topRight);
        Gizmos.DrawRay(currentCamera.transform.position, bottomRight);
        Gizmos.DrawRay(currentCamera.transform.position, bottomLeft);
    }


}
