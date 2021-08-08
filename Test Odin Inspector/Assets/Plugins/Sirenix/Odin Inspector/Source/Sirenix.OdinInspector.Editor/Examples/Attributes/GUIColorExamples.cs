#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUIColorExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(GUIColorAttribute))]
    internal class GUIColorExamples
    {
        [GUIColor(0.3f, 0.8f, 0.8f, 1f)]
        public int ColoredInt1;

        [GUIColor(0.3f, 0.8f, 0.8f, 1f)]
        public int ColoredInt2;

        [ButtonGroup]
        [GUIColor(0, 1, 0)]
        private void Apply()
        {
        }

        [ButtonGroup]
        [GUIColor(1, 0.6f, 0.4f)]
        private void Cancel()
        {
        }

        [InfoBox("You can also reference a color member to dynamically change the color of a property.")]
        [GUIColor("GetButtonColor")]
        [Button("I Am Fabulous", ButtonSizes.Gigantic)]
        private static void IAmFabulous()
        {
        }

        [Button(ButtonSizes.Large)]
        [GUIColor("@Color.Lerp(Color.red, Color.green, Mathf.Abs(Mathf.Sin((float)EditorApplication.timeSinceStartup)))")]
        private static void Expressive()
        {
        }

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        private static Color GetButtonColor()
        {
            Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
            return Color.HSVToRGB(Mathf.Cos((float)UnityEditor.EditorApplication.timeSinceStartup + 1f) * 0.225f + 0.325f, 1, 1);
        }
#endif
    }
}
#endif