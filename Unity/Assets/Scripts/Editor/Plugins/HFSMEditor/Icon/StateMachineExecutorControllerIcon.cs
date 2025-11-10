#if UNITY_EDITOR
using NLog.Fluent;
using UnityEditor;
using UnityEngine;

namespace ZHFSM
{
    [CustomEditor(typeof(StateMachineExecutorController))]
    public class StateMachineExecutorControllerEditor : Editor
    {
        // 空实现，只是为了显示图标
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }

    // 为资源设置图标
    [InitializeOnLoad]
    public static class StateMachineExecutorControllerIcon
    {
        static StateMachineExecutorControllerIcon()
        {
            EditorApplication.delayCall  += ApplyCustomIcon;
        }

        private static void ApplyCustomIcon()
        {
          
            string[] guids = AssetDatabase.FindAssets("t:StateMachineExecutorController");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null)
                {
                    string iconPath = string.Empty;

                    // 方法一：通过 MonoScript 获取
                    MonoScript monoScript = MonoScript.FromScriptableObject(asset as StateMachineExecutorController);
                    if (monoScript != null)
                    {
                        iconPath = AssetDatabase.GetAssetPath(monoScript).Replace(".cs", ".png");
                    }
                    var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                    EditorGUIUtility.SetIconForObject(asset, icon);
                }
            }
          
        }


    }
}

#endif