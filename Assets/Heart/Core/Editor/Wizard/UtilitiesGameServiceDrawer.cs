using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesGameServiceDrawer
    {
        public static void OnInspectorGUI()
        {
#if !PANCAKE_LEADERBOARD
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Package For Leaderboard", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.unity.services.leaderboards", "1.0.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

#if !PANCAKE_CLOUDSAVE
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Package For Backup/Restore", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.unity.services.cloudsave", "3.0.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif


            GUILayout.FlexibleSpace();
#if PANCAKE_LEADERBOARD
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Leaderboard", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Leaderboard", "Are you sure you want to uninstall leaderboard package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.unity.services.leaderboards");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#endif

#if PANCAKE_CLOUDSAVE
#endif
        }
    }
}