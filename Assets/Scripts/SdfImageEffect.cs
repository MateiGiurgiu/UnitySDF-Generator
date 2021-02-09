using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SdfImageEffect : MonoBehaviour
{
    [SerializeField] private Shader m_sdfVisualiserShader = null;
    [SerializeField] private Texture3D m_sdfTexture = null;

    [SerializeField, Range(0.25f, 10f)] private float m_sdfScale = 1f;
    [SerializeField] private Vector3 m_sdfPosition = Vector3.zero;

    private Material m_imageEffectMaterial = null;
    private Material ImageEffectMaterial
    {
        get
        {
            if (!m_imageEffectMaterial && m_sdfVisualiserShader)
            {
                m_imageEffectMaterial = new Material(m_sdfVisualiserShader);
                m_imageEffectMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_imageEffectMaterial;
        }
    }

    private Camera m_currentCamera = null;
    private Camera CurrentCamera
    {
        get
        {
            if (!m_currentCamera)
            {
                m_currentCamera = gameObject.GetComponent<Camera>();
            }
            return m_currentCamera;
        }
    }


    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!ImageEffectMaterial || !m_sdfTexture)
        {
            // simply replace the source texture with the destination's contents without performing any processing
            Graphics.Blit(source, destination);
        }
        else
        {
            // pass data to the shader
            ImageEffectMaterial.SetTexture("_SDF", m_sdfTexture);
            ImageEffectMaterial.SetMatrix("_SDFMappingMatrix", GetVolumeMappingFunction());
            ImageEffectMaterial.SetMatrix("_FrustumCornersMatrix", GetFrustumCorners());
            ImageEffectMaterial.SetFloat("_SdfScale", m_sdfScale);
            ImageEffectMaterial.SetVector("_SdfPosition", m_sdfPosition);
            ImageEffectMaterial.SetMatrix("_CameraInvViewMatrix", CurrentCamera.cameraToWorldMatrix);
            CustomGraphicsBlit(source, destination, ImageEffectMaterial, 0);
            // copy the source texture into the destination using a material. 
            // the shader attached to this material will perform the sphere-marching algorithm
            //Graphics.Blit(source, destination, ImageEffectMaterial, 0);
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
        GL.Vertex3(0.0f, 0.0f, 0.0f); // Bottom Left

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 1.0f); // Top Left

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 2.0f); // Top Right

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 3.0f); // Bottom Right

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 GetVolumeMappingFunction()
    {
        Matrix4x4 scale = Matrix4x4.Scale(Vector3.one * (1f / m_sdfScale));
        Matrix4x4 translate = Matrix4x4.Translate(-m_sdfPosition + Vector3.one * (m_sdfScale / 2f));
        return scale * translate;
    }

    private Matrix4x4 GetFrustumCorners()
    {
        float camFov = CurrentCamera.fieldOfView;
        float camAspect = CurrentCamera.aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float halfFov = camFov * 0.5f;
        float tanFov = Mathf.Tan(halfFov * Mathf.Deg2Rad);

        Vector3 toRight = Vector3.right * tanFov * camAspect;
        Vector3 toTop = Vector3.up * tanFov;

        Vector3 bottomLeft = Vector3.Normalize(-Vector3.forward - toRight - toTop);
        Vector3 topLeft = Vector3.Normalize(-Vector3.forward - toRight + toTop);
        Vector3 topRight = Vector3.Normalize(-Vector3.forward + toRight + toTop);
        Vector3 bottomRight = Vector3.Normalize(-Vector3.forward + toRight - toTop);

        frustumCorners.SetRow(0, bottomLeft);
        frustumCorners.SetRow(1, topLeft);
        frustumCorners.SetRow(2, topRight);
        frustumCorners.SetRow(3, bottomRight);

        return frustumCorners;
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 frustumCorners = GetFrustumCorners();

        Vector3 topLeft = CurrentCamera.cameraToWorldMatrix.MultiplyVector(frustumCorners.GetRow(0));
        Vector3 topRight = CurrentCamera.cameraToWorldMatrix.MultiplyVector(frustumCorners.GetRow(1));
        Vector3 bottomRight = CurrentCamera.cameraToWorldMatrix.MultiplyVector(frustumCorners.GetRow(2));
        Vector3 bottomLeft = CurrentCamera.cameraToWorldMatrix.MultiplyVector(frustumCorners.GetRow(3));

        Gizmos.color = Color.green;

        Gizmos.DrawRay(CurrentCamera.transform.position, topLeft);
        Gizmos.DrawRay(CurrentCamera.transform.position, topRight);
        Gizmos.DrawRay(CurrentCamera.transform.position, bottomRight);
        Gizmos.DrawRay(CurrentCamera.transform.position, bottomLeft);
    }
}