#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinEditorDefinitions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using UnityEditor;
    using Sirenix.Serialization;

    // Make sure Odin is used to draw our config, even if automatic Odin Editor is disabled globally.

    [CustomEditor(typeof(GeneralDrawerConfig))]
    internal class GeneralDrawerConfigEditor : OdinEditor
    {
    }

    [CustomEditor(typeof(InspectorConfig))]
    internal class InspectorConfigEditor : OdinEditor
    {
    }

    [CustomEditor(typeof(EditorOnlyModeConfig))]
    internal class EditorOnlyModeConfigEditor : OdinEditor
    {
    }

    [CustomEditor(typeof(PersistentContextCache))]
    internal class PersistentContextCacheEditor : OdinEditor
    {
    }

    [CustomEditor(typeof(ColorPaletteManager))]
    internal class ColorPaletteManagerEditor : OdinEditor
    {
    }

    [CustomEditor(typeof(AOTGenerationConfig))]
    internal class AOTGenerationConfigEditor : OdinEditor
    {
    }

    [CustomEditor(typeof(GlobalSerializationConfig))]
    internal class GlobalSerializationConfigEditor : OdinEditor
    {
    }
}
#endif