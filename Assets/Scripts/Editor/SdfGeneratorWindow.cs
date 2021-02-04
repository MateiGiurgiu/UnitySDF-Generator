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

public class SdfGeneratorWindow : EditorWindow
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
        SdfGeneratorWindow window = (SdfGeneratorWindow)EditorWindow.GetWindow(typeof(SdfGeneratorWindow));
        window.Show();
    }

    public void OnGUI()
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
            Texture3D sdf = SdfGenerator.GenerateSdf(m_mesh, m_sdfResolution);
            Save3DTexture(sdf);
        }
        GUI.enabled = true;
        /*
        if (GUILayout.Button("Check Bounds", GUILayout.MaxWidth(150f)))
        {
            m_mesh.RecalculateBounds();
            Debug.Log($"Center: {m_mesh.bounds.center.ToString("F4")} | Size: {m_mesh.bounds.size.ToString("F4")} | Extents: {m_mesh.bounds.extents.ToString("F4")} | Min: {m_mesh.bounds.min.ToString("F4")} | Max: {m_mesh.bounds.max.ToString("F4")}");
        }*/
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void Save3DTexture(Texture3D texture)
    {
        string path = Path.Combine(m_savePath, $"{m_mesh.name}_SDF_{m_sdfResolution}.asset");
        AssetDatabase.CreateAsset(texture, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
