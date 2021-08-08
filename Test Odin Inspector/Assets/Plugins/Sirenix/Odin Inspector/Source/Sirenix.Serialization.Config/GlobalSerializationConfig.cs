//-----------------------------------------------------------------------
// <copyright file="GlobalSerializationConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.Utilities;
    using Sirenix.OdinInspector;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    [HideMonoScript]
    [SirenixGlobalConfig]
    public class GlobalSerializationConfig : GlobalConfig<GlobalSerializationConfig>
    {
        /// <summary>
        /// Text for the cautionary serialization warning shown in the inspector.
        /// </summary>
        public const string ODIN_SERIALIZATION_CAUTIONARY_WARNING_TEXT =
            "Odin's custom serialization protocol is stable and fast. It is built to be fast, reliable and resilient above all." +
            "\n\n" +
            "*Words of caution* \n" +
            "However, caveats apply - there is a reason Unity chose such a drastically limited serialization protocol. " +
            "It keeps things simple and manageable, and limits how much complexity you can introduce into your data structures. " +
            "It can be very easy to get carried away and shoot yourself in the foot when all limitations suddenly disappear, " +
            "and hence we have included this cautionary warning." +
            "\n\n" +
            "Warning words aside, there can of course be valid reasons to use a more powerful serialization protocol such as Odin's. " +
            "However, we advise you to use it wisely and with restraint. After all, with great power comes great responsibility!";

        public const string ODIN_PREFAB_CAUTIONARY_WARNING_TEXT =
            "In 2018.3, Unity introduced a new prefab workflow, and in so doing, changed how all prefabs fundamentally work. " +
            "Despite our best efforts, we have so far been unable to achieve a stable implementation of Odin-serialized prefab " +
            "modifications on prefab instances and variants in the new prefab workflow." +
            "" +
            "This has nothing to do with Odin serializer itself, which remains rock solid. Odin-serialized ScriptableObjects " +
            "and non-prefab Components/Behaviours are still perfectly stable - you are only seeing this message because this " +
            "is an Odin-serialized prefab asset or instance." +
            "\n\n" +
            "Using prefabs with Odin serialization in 2018.3 and above is considered a *deprecated feature* and is officially " +
            "unsupported. In short, if you disregard this message and then experience issues, we will not be able to help or " +
            "support you." +
            "\n\n" +
            "Please keep all this in mind, if you wish to continue using Odin-serialized prefabs.";

        /// <summary>
        /// Text for the hide button for the cautionary serialization warning shown in the inspector.
        /// </summary>
        public const string ODIN_SERIALIZATION_CAUTIONARY_WARNING_BUTTON_TEXT = "I know what I'm about, son. Hide message forever.";

        /// <summary>
        /// Text for the hide button for the cautionary prefab warning shown in the inspector.
        /// </summary>
        public const string ODIN_PREFAB_CAUTIONARY_WARNING_BUTTON_TEXT = "I understand that I'm on my own. Hide message forever.";

#pragma warning disable 0414

        private static readonly DataFormat[] BuildFormats = new DataFormat[] { DataFormat.Binary, DataFormat.JSON };

#pragma warning restore 0414

        /// <summary>
        /// Whether the user has chosen to hide the cautionary serialization warning.
        /// </summary>
        [Title("Warning messages")]
        [ToggleLeft]
        [DetailedInfoBox("Click to show warning message.", ODIN_SERIALIZATION_CAUTIONARY_WARNING_TEXT)]
        public bool HideSerializationCautionaryMessage = false;

        //[EnableIf("@OdinPrefabSerializationEditorUtility.HasNewPrefabWorkflow")]
        [ToggleLeft]
        [DetailedInfoBox("Click to show warning message.", ODIN_PREFAB_CAUTIONARY_WARNING_TEXT)]
        public bool HidePrefabCautionaryMessage = false;

        /// <summary>
        /// Whether the user has chosen to hide the warning messages related to the OdinSerialize attribute.
        /// </summary>
        [ToggleLeft]
        [SerializeField]
        [InfoBox("Enabling this will hide all warning messages that will show up in the inspector when the OdinSerialize attribute potentially does not achieve the desired effect.")]
        public bool HideOdinSerializeAttributeWarningMessages = false;

        /// <summary>
        /// Whether the user has chosen to hide the warning messages related to the SerializeField and ShowInInspector attributes on non-serialized members.
        /// </summary>
        [SerializeField]
        [ToggleLeft, LabelText("Hide Non-Serialized SerializeField/ShowInInspector Warning Messages")]
        [InfoBox("Enabling this will hide all warning messages that show up when the SerializeField and the ShowInInspector attributes are used together on non-serialized fields or properties.")]
        public bool HideNonSerializedShowInInspectorWarningMessages = false;

        [SerializeField, Title("Data formatting options"), ValueDropdown("BuildFormats")]
        private DataFormat buildSerializationFormat = DataFormat.Binary;

        [SerializeField]
        private DataFormat editorSerializationFormat = DataFormat.Nodes;

        [SerializeField, Title("Logging and error handling")]
        private LoggingPolicy loggingPolicy = LoggingPolicy.LogErrors;

        [SerializeField]
        private ErrorHandlingPolicy errorHandlingPolicy = ErrorHandlingPolicy.Resilient;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public ILogger Logger { get { return DefaultLoggers.UnityLogger; } }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public DataFormat EditorSerializationFormat { get { return this.editorSerializationFormat; } set { this.editorSerializationFormat = value; } }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public DataFormat BuildSerializationFormat { get { return this.buildSerializationFormat; } set { this.buildSerializationFormat = value; } }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public LoggingPolicy LoggingPolicy { get { return this.loggingPolicy; } set { this.loggingPolicy = value; } }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public ErrorHandlingPolicy ErrorHandlingPolicy { get { return this.errorHandlingPolicy; } set { this.errorHandlingPolicy = value; } }

        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            var boldStyle = new GUIStyle(GUI.skin.label) { richText = true };

            var boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.padding = new RectOffset(7, 7, 7, 7);

            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.clipping = TextClipping.Overflow;
            labelStyle.wordWrap = true;

            GUILayout.Space(20);

            GUILayout.BeginVertical(boxStyle);
            {
                GUILayout.Label("<b>Serialization Formats</b>", boldStyle);
                GUILayout.Label("The serialization format of the data in specially serialized Unity objects. Binary is recommended for builds; JSON has the benefit of being human-readable but has significantly worse performance.\n\nWith the special editor-only node format, the serialized data will be formatted in such a way that, if the asset is saved with Unity's text format (Edit -> Project Settings -> Editor -> Asset Serialization -> Mode), the data will be mergeable when using version control systems. This makes the custom serialized data a lot less fragile, but comes at a performance cost during serialization and deserialization. The node format is recommended in the editor.\n\nThis setting can be overridden on a per-instance basis.\n", labelStyle);

                GUILayout.Label("<b>Error Handling Policy</b>", boldStyle);
                GUILayout.Label("The policy for handling any errors and irregularities that crop up during deserialization. Resilient is the recommended option, as it will always try to recover as much data as possible from a corrupt serialization stream.\n", labelStyle);

                GUILayout.Label("<b>Logging Policy</b>", boldStyle);
                GUILayout.Label("Use this to determine the criticality of the events that are logged by the serialization system. Recommended value is to log only errors, and to log warnings and errors when you suspect issues in the system.", labelStyle);
            }
            GUILayout.EndVertical();
        }
    }
}