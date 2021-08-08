#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Utilities;
    using UnityEngine;
    using Sirenix.Utilities.Editor;
    using Sirenix.Serialization;
    using UnityEditor;
    using System.IO;
    using System;

    /// <summary>
    /// <para>
    /// Tell Odin which types should be drawn or should not be drawn by Odin.
    /// </para>
    /// <para>
    /// You can modify which types should be drawn by Odin in the Preferences window found in 'Tools -> Odin Inspector -> Preferences -> Editor Types',
    /// or by locating the configuration file stored as a serialized object in the Sirenix folder under 'Odin Inspector/Config/Editor/InspectorConfig'.
    /// </para>
    /// </summary>
    [SirenixEditorConfig]
    public class InspectorConfig : GlobalConfig<InspectorConfig>, ISerializationCallbackReceiver
    {
        // TODO: Generated ODin Editor DLLs might be soo old now that this can be removed?
        [InitializeOnLoadMethod]
        private static void RemoveObsoleteGeneratedOdinEditorsDLL()
        {
            UnityEditorEventUtility.EditorApplication_delayCall += () =>
            {
                UnityEditorEventUtility.EditorApplication_delayCall += () =>
                {
#if SIRENIX_INTERNAL
                    if (AssemblyUtilities.GetTypeByCachedFullName("Sirenix.Internal.SirenixProduct") != null)
                    {
                        return;
                    }
#endif
                    if (EditorPrefs.HasKey("PREVENT_SIRENIX_FILE_GENERATION"))
                    {
                        return;
                    }

                    var generatedOdinEditorsFolder = SirenixAssetPaths.SirenixAssembliesPath + "Editor";
                    var generatedOdinEditorsDLL = generatedOdinEditorsFolder + "/GeneratedOdinEditors.dll";

                    if (File.Exists(generatedOdinEditorsDLL))
                    {
                        AssetDatabase.DeleteAsset(generatedOdinEditorsDLL);
                        if (File.Exists(generatedOdinEditorsDLL + ".mdb"))
                        {
                            AssetDatabase.DeleteAsset(generatedOdinEditorsDLL + ".mdb");
                        }

                        AssetDatabase.Refresh();
                    }
                };
            };
        }

        private static bool hasUpdatedEditorsOnce = false;

        [Space(5)]
        [SerializeField, HorizontalGroup, OnValueChanged("UpdateOdinEditors")]
        [ToggleLeft, LabelText(" Enable Odin In Inspector"), Tooltip("Whether Odin is enabled in the inspector or not.")]
        private bool enableOdinInInspector = true;

        [SerializeField, HideInInspector]
        private InspectorDefaultEditors defaultEditorBehaviour = InspectorDefaultEditors.UserTypes | InspectorDefaultEditors.PluginTypes | InspectorDefaultEditors.OtherTypes;

        [SerializeField, HideInInspector]
        private bool processMouseMoveInInspector = true;

        [SerializeField, DisableContextMenu(true, true)]
        private InspectorTypeDrawingConfig drawingConfig = new InspectorTypeDrawingConfig();

        private void SuppressMissingEditorTypeErrorsMessage()
        {
            if (UnityVersion.Major == 2017 && UnityVersion.Minor == 1)
            {
                SirenixEditorGUI.ErrorMessageBox("Suppressing these error messages may cause crashes on Unity 2017.1 (see Unity issue 920772). A fix is being backported from 2017.2 - meanwhile, you may want to disable this option, and live with the constant error messages about missing editor types.");
            }
        }

        /// <summary>
        /// Whether Odin is enabled in the inspector or not.
        /// </summary>
        public bool EnableOdinInInspector
        {
            get
            {
                return this.enableOdinInInspector;
            }
            set
            {
                if (value != this.enableOdinInInspector)
                {
                    this.enableOdinInInspector = value;
                    this.UpdateOdinEditors();
                }
            }
        }

        /// <summary>
        /// InspectorDefaultEditors is a bitmask used to tell which types should have an Odin Editor generated.
        /// </summary>
        public InspectorDefaultEditors DefaultEditorBehaviour
        {
            get { return this.defaultEditorBehaviour; }
            set { this.defaultEditorBehaviour = value; }
        }

        /// <summary>
        /// The config which contains configuration data for which types Odin should draw in the inspector.
        /// </summary>
        public InspectorTypeDrawingConfig DrawingConfig { get { return this.drawingConfig; } }

        internal bool ProcessMouseMoveInInspector
        {
            get { return this.processMouseMoveInInspector; }
            set { this.processMouseMoveInInspector = value; }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.drawingConfig.UpdateCaches();
            UnityEditorEventUtility.DelayAction(() => this.UpdateOdinEditors());
        }

        /// <summary>
        /// Updates Unity with the current Odin editor configuration.
        /// </summary>
        [Button("Update Editors", buttonSize: 22), HorizontalGroup(width: 100)]
        public void UpdateOdinEditors()
        {
            CustomEditorUtility.ResetCustomEditors();

            if (this.enableOdinInInspector)
            {
                foreach (var typeDrawerPair in InspectorTypeDrawingConfigDrawer.GetEditors())
                {
                    var drawnType = TwoWaySerializationBinder.Default.BindToType(typeDrawerPair.DrawnTypeName);
                    var editorType = TwoWaySerializationBinder.Default.BindToType(typeDrawerPair.EditorTypeName);

                    if (drawnType == null || editorType == null) continue;

                    CustomEditorUtility.SetCustomEditor(drawnType, editorType, isFallbackEditor: false, isEditorForChildClasses: false);
                }
            }

            Type inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            Type activeEditorTrackerType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ActiveEditorTracker");

            if (inspectorWindowType != null && activeEditorTrackerType != null)
            {
                var createTrackerMethod = inspectorWindowType.GetMethod("CreateTracker", Flags.InstanceAnyVisibility);
                var trackerField = inspectorWindowType.GetField("m_Tracker", Flags.InstanceAnyVisibility);
                var forceRebuild = activeEditorTrackerType.GetMethod("ForceRebuild", Flags.InstanceAnyVisibility);

                if (createTrackerMethod != null && trackerField != null && forceRebuild != null)
                {
                    var windows = Resources.FindObjectsOfTypeAll(inspectorWindowType);

                    foreach (var window in windows)
                    {
                        createTrackerMethod.Invoke(window, null);
                        object tracker = trackerField.GetValue(window);
                        forceRebuild.Invoke(tracker, null);
                    }
                }
            }

            hasUpdatedEditorsOnce = true;
        }

        internal void EnsureEditorsHaveBeenUpdated()
        {
            if (hasUpdatedEditorsOnce == false)
            {
                this.UpdateOdinEditors();
            }
        }

        [OnInspectorGUI]
        private void BottomSpace()
        {
            GUILayout.Space(10);
        }
    }
}
#endif