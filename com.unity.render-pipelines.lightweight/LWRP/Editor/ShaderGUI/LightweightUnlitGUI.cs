using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class LightweightUnlitGUI : LightweightShaderGUI
{
    private MaterialProperty mainTexProp;
    private MaterialProperty mainColorProp;
    private MaterialProperty sampleGIProp;
    private MaterialProperty bumpMap;

    private static class Styles
    {
        public static GUIContent[] mainTexLabels =
        {
            new GUIContent("MainTex (RGB)", "Base Color"),
            new GUIContent("MainTex (RGB) Alpha (A)", "Base Color and Alpha")
        };

        public static string surfaceProperties = "Surface Properties";
        public static GUIContent normalMapLabel = new GUIContent("Normal Map", "Normal Map");
        public static GUIContent sampleGILabel = new GUIContent("Sample GI", "If enabled GI will be sampled from SH or Lightmap.");
    }

    public override void FindProperties(MaterialProperty[] properties)
    {
        base.FindProperties(properties);
        mainTexProp = FindProperty("_MainTex", properties);
        mainColorProp = FindProperty("_Color", properties);
        sampleGIProp = FindProperty("_SampleGI", properties, false);
        bumpMap = FindProperty("_BumpMap", properties, false);
    }

    public override void ShaderPropertiesGUI(Material material)
    {
        EditorGUI.BeginChangeCheck();
        {
            base.ShaderPropertiesGUI(material);
            GUILayout.Label(Styles.surfaceProperties, EditorStyles.boldLabel);
            int surfaceTypeValue = (int)surfaceTypeProp.floatValue;
            if (alphaClipProp.floatValue >= 1.0f)
                surfaceTypeValue = 1;
            GUIContent mainTexLabel = Styles.mainTexLabels[Math.Min(surfaceTypeValue, 1)];
            m_MaterialEditor.TexturePropertySingleLine(mainTexLabel, mainTexProp, mainColorProp);

            EditorGUILayout.Space();
            m_MaterialEditor.ShaderProperty(sampleGIProp, Styles.sampleGILabel);
            if (sampleGIProp.floatValue >= 1.0)
                m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapLabel, bumpMap);

            m_MaterialEditor.TextureScaleOffsetProperty(mainTexProp);
        }
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var target in blendModeProp.targets)
                MaterialChanged((Material)target);
        }

        DoMaterialRenderingOptions();
    }

    public override void MaterialChanged(Material material)
    {
        material.shaderKeywords = null;
        SetupMaterialBlendMode(material);
        SetMaterialKeywords(material);
    }

    static void SetMaterialKeywords(Material material)
    {
        bool sampleGI = material.GetFloat("_SampleGI") >= 1.0f;
        CoreUtils.SetKeyword(material, "_SAMPLE_GI", sampleGI);
        CoreUtils.SetKeyword(material, "_NORMAL_MAP", sampleGI && material.GetTexture("_BumpMap"));
    }
}
