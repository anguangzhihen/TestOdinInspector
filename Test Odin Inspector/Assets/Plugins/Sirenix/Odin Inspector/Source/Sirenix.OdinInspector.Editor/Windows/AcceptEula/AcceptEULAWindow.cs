#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using UnityEditor;
    using UnityEngine;

#if !ODIN_TRIAL
    internal class AcceptEULAWindow : OdinEditorWindow
    {
        private static int WIDTH = 600;
        private static int HEIGHT = 244;

        private static bool IsHeadlessMode { get { return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null; } }

        public static string HAS_ACCEPTED_EULA_PREFS_KEY = "ACCEPTED_ODIN_3_0_PERSONAL_EULA";

        public static bool HasAcceptedEULA { get { return EditorPrefs.GetBool(HAS_ACCEPTED_EULA_PREFS_KEY, false); } }

        [InitializeOnLoadMethod]
        private static void OpenIfNotAccepted()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (HasAcceptedEULA || IsHeadlessMode || UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                EditorApplication.update -= OnUpdate;
                return;
            }

            if (EditorApplication.isCompiling) return;

            try
            {
                var window = GetWindow<AcceptEULAWindow>();

                window.hasReadAndUnderstood = false;
                window.isUnderRevenueCap = false;
                window.name = "EULA Agreement Needed";

                if (UnityVersion.IsVersionOrGreater(2019, 3))
                {
                    HEIGHT += 2;
                }

                window.Show();
                window.minSize = new Vector2(WIDTH, HEIGHT);
                window.maxSize = new Vector2(WIDTH, HEIGHT);
                window.titleContent = new GUIContent("Odin EULA");
                window.position = GUIHelper.GetEditorWindowRect().AlignCenter(WIDTH, HEIGHT);

                EditorApplication.update -= OnUpdate;
            }
            catch (Exception ex)
            {
                EditorApplication.update -= OnUpdate;
                Debug.LogException(new Exception("An exception happened while attempting to open Odin's EULA popup window.", ex));
            }
        }

        [OnInspectorGUI, PropertyOrder(-1)]
        private void DrawEULA()
        {
            GUI.color = new Color(1, 1, 1, 0.15f);
            GUI.DrawTexture(new Rect(WIDTH - HEIGHT * 1.0f, HEIGHT * 0.0f, HEIGHT * 1.0f, HEIGHT * 1.0f), EditorIcons.OdinInspectorLogo, ScaleMode.ScaleToFit, true);
            GUI.color = Color.white;

            var titleLabel = new GUIContent("Odin Personal EULA");

            GUILayout.Label(titleLabel, SirenixGUIStyles.SectionHeaderCentered);

            var titleRect = GUILayoutUtility.GetLastRect();
            var titleWidth = SirenixGUIStyles.SectionHeaderCentered.CalcSize(titleLabel).x;

            GUI.DrawTexture(titleRect.AlignCenter(40, 40).AddX(-25 + titleWidth * -0.5f), EditorIcons.OdinInspectorLogo, ScaleMode.ScaleToFit);

            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            GUILayout.Label(
@"In order to use Odin Personal, you must read and accept the Odin Personal EULA!

Most notably, the EULA restricts the use of the Odin Personal license by people or entities with revenue or funding in excess of $200,000 USD in the past 12 months.", SirenixGUIStyles.MultiLineLabel);
            GUILayout.Space(8);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Read the full EULA", GUILayoutOptions.Height(31).Width(130)))
            {
                Application.OpenURL("https://odininspector.com/eula");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.Space(10);
        }


        [BoxGroup("Box", LabelText = "EULA Agreement Needed", CenterLabel = true)]
        [ShowInInspector, ToggleLeft]
        [LabelText(@"I have read and understood the EULA, and the restrictions that apply to the use of Odin Personal")]
        private bool hasReadAndUnderstood;

        [BoxGroup("Box")]
        [ShowInInspector, ToggleLeft]
        [LabelText(@"I or the entity I work for had less than $200,000 USD revenue or funding in the past 12 months")]
        private bool isUnderRevenueCap;

        [PropertySpace(5)]
        [BoxGroup("Box"), Button(ButtonSizes.Large), LabelText("I agree to the EULA"), EnableIf("@hasReadAndUnderstood && isUnderRevenueCap")]
        private void Agree()
        {
            EditorPrefs.SetBool(HAS_ACCEPTED_EULA_PREFS_KEY, true);
            GetWindow<AcceptEULAWindow>().Close();
        }
    }

#if !ODIN_TRIAL && !ODIN_ENTERPRISE && SIRENIX_INTERNAL
    internal static class INTERNAL_RemoveEULAConsent
    {
        [MenuItem("Sirenix/Utilities/Remove EULA Consent")]
        private static void RemoveEULAConsent()
        {
            EditorPrefs.SetBool(AcceptEULAWindow.HAS_ACCEPTED_EULA_PREFS_KEY, false);
        }
    }
#endif
#endif
}
#endif