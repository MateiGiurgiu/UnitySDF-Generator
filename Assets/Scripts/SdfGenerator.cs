

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum PowerOf8Size
{
    _8 = 8,
    _16 = 16,
    _32 = 32,
    _64 = 64,
    _96 = 96,
    _128 = 128,
    _160 = 160,
    _200 = 200,
    _256 = 256
};

public class SdfGenerator : EditorWindow
{
    private string m_savePath
    {
        get => EditorPrefs.GetString("SdfGenerator_SavePath", string.Empty);
        set => EditorPrefs.SetString("SdfGenerator_SavePath", value);
    }
    private int m_sdfResolution = 32;
    private Mesh m_mesh;


    [MenuItem("Utilities/SDF Generator")]
    static void Init()
    {
        SdfGenerator window = (SdfGenerator)EditorWindow.GetWindow(typeof(SdfGenerator));
        window.Show();
        //SceneView.duringSceneGui += window.OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        // title
        EditorGUILayout.Space();
        GUILayout.Label("--- SDF Generator ---", new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        });
        EditorGUILayout.Space();

        // save location for the SDF
        GUILayout.BeginHorizontal();
        {
            GUI.enabled = false;
            EditorGUILayout.TextField("Save Location", m_savePath, GUILayout.MaxWidth(600));
            GUI.enabled = true;

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Folder Icon"), GUILayout.MaxWidth(40), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)))
            {
                m_savePath = EditorUtility.SaveFolderPanel("SDF Save Location", m_savePath, "").Substring(Application.dataPath.Length).Insert(0, "Assets");
            }
        }
        GUILayout.EndHorizontal();

        // resolution of the sdf
        m_sdfResolution = Convert.ToInt32((PowerOf8Size)EditorGUILayout.EnumPopup("SDF Resolution", (PowerOf8Size)m_sdfResolution));

        // the mesh to compute the SDF from
        m_mesh = EditorGUILayout.ObjectField(new GUIContent("Mesh"), m_mesh, typeof(Mesh), false) as Mesh;

        EditorGUILayout.Space();

        // show the button as not interactable if there is no mesh assigned
        GUI.enabled = (m_mesh != null);

        // use horizontal layout and flexible space to center the button horizontally
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Generate SDF", GUILayout.MaxWidth(150f)))
        {
            GenerateSDF();
        }
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private float ComputeSize(Bounds meshBounds)
    {
        float size = 0f;

        // pick the largest value of all sides
        size = Mathf.Max(meshBounds.size.x, meshBounds.size.y, meshBounds.size.z);

        return size;
    }


    private void GenerateSDF()
    {
        //RenderTexture rt = new RenderTexture(SdfSize, SdfSize, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        //rt.volumeDepth = SdfSize;
        //rt.enableRandomWrite = true;
        //rt.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        //rt.wrapMode = TextureWrapMode.Clamp;
        //rt.Create();

        Texture3D tex3D = new Texture3D(m_sdfResolution, m_sdfResolution, m_sdfResolution, TextureFormat.RGBAHalf, false);
        tex3D.anisoLevel = 1;
        tex3D.filterMode = FilterMode.Bilinear;
        tex3D.wrapMode = TextureWrapMode.Clamp;

        Save3DTexture(tex3D);
    }

    private void Save3DTexture(Texture3D texture)
    {
        //string path = Path.Combine(m_savePath, m_mesh.name + "_SDF.asset");
        string path = m_savePath + "/" + m_mesh.name + "_SDF.asset";
        AssetDatabase.CreateAsset(texture, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


}
