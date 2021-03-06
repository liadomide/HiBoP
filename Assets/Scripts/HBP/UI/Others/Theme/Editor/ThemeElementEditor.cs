﻿using UnityEditor;
using UnityEngine;

namespace HBP.UI.Theme
{
    [CustomEditor(typeof(OldThemeElement))]
    [CanEditMultipleObjects]
    public class ThemeElementEditor : Editor
    {
        SerializedProperty m_IgnoreThemeProp;
        SerializedProperty m_ZoneProp;
        SerializedProperty m_GeneralProp;
        SerializedProperty m_MenuProp;
        SerializedProperty m_WindowProp;
        SerializedProperty m_HeaderProp;
        SerializedProperty m_ContentProp;
        SerializedProperty m_ItemProp;
        SerializedProperty m_ToolbarProp;
        SerializedProperty m_VisualizationProp;
        SerializedProperty m_EffectProp;
        SerializedProperty m_GraphicsProp;

        private void OnEnable()
        {
            m_IgnoreThemeProp = serializedObject.FindProperty("IgnoreTheme");
            m_ZoneProp = serializedObject.FindProperty("Zone");
            m_GeneralProp = serializedObject.FindProperty("General");
            m_MenuProp = serializedObject.FindProperty("Menu");
            m_WindowProp = serializedObject.FindProperty("Window");
            m_HeaderProp = serializedObject.FindProperty("Header");
            m_ContentProp = serializedObject.FindProperty("Content");
            m_ItemProp = serializedObject.FindProperty("Item");
            m_ToolbarProp = serializedObject.FindProperty("Toolbar");
            m_VisualizationProp = serializedObject.FindProperty("Visualization");
            m_EffectProp = serializedObject.FindProperty("Effect");
            m_GraphicsProp = serializedObject.FindProperty("Graphics");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_IgnoreThemeProp);
            if (!m_IgnoreThemeProp.boolValue)
            {
                EditorGUILayout.PropertyField(m_ZoneProp);
                switch ((OldThemeElement.ZoneEnum) m_ZoneProp.enumValueIndex)
                {
                    case OldThemeElement.ZoneEnum.General: EditorGUILayout.PropertyField(m_GeneralProp, new GUIContent("Type")); break;
                    case OldThemeElement.ZoneEnum.Menu: EditorGUILayout.PropertyField(m_MenuProp, new GUIContent("Type")); break;
                    case OldThemeElement.ZoneEnum.Window:
                        EditorGUILayout.PropertyField(m_WindowProp, new GUIContent("Window"));
                        switch ((OldThemeElement.WindowEnum)m_WindowProp.enumValueIndex)
                        {
                            case OldThemeElement.WindowEnum.Header: EditorGUILayout.PropertyField(m_HeaderProp, new GUIContent("Type")); break;
                            case OldThemeElement.WindowEnum.Content:
                                EditorGUILayout.PropertyField(m_ContentProp, new GUIContent("Type"));
                                if ((OldThemeElement.ContentEnum) m_ContentProp.enumValueIndex == OldThemeElement.ContentEnum.Item)
                                {
                                    EditorGUILayout.PropertyField(m_ItemProp, new GUIContent("Type"));
                                }
                                break;
                        }
                        break;
                    case OldThemeElement.ZoneEnum.Toolbar: EditorGUILayout.PropertyField(m_ToolbarProp, new GUIContent("Type")); break;
                    case OldThemeElement.ZoneEnum.Visualization: EditorGUILayout.PropertyField(m_VisualizationProp, new GUIContent("Type")); break;
                }
                EditorGUILayout.PropertyField(m_EffectProp, new GUIContent("Effect"));
                if ((OldThemeElement.EffectEnum)m_EffectProp.enumValueIndex == OldThemeElement.EffectEnum.Custom)
                {
                    EditorGUILayout.PropertyField(m_GraphicsProp,new GUIContent("Graphics"),true);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}