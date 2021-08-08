#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TransposeTableMatrixExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.Utilities;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [ShowOdinSerializedPropertiesInInspector]
    [AttributeExample(typeof(TableMatrixAttribute), Name = "Transpose")]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.Utilities" })]
    internal class TransposeTableMatrixExample
    {
        [TableMatrix(HorizontalTitle = "Custom Cell Drawing", DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false, RowHeight = 16)]
        public bool[,] CustomCellDrawing;

        [ShowInInspector, DoNotDrawAsReference]
        [TableMatrix(HorizontalTitle = "Transposed Custom Cell Drawing", DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false, RowHeight = 16, Transpose = true)]
        public bool[,] Transposed { get { return CustomCellDrawing; } set { CustomCellDrawing = value; } }

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        private static bool DrawColoredEnumElement(Rect rect, bool value)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
            }

            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), value ? new Color(0.1f, 0.8f, 0.2f) : new Color(0, 0, 0, 0.5f));

            return value;
        }

        [OnInspectorInit]
        private void CreateData()
        {
            // =)
            this.CustomCellDrawing = new bool[15, 15];
            this.CustomCellDrawing[6, 5] = true;
            this.CustomCellDrawing[6, 6] = true;
            this.CustomCellDrawing[6, 7] = true;
            this.CustomCellDrawing[8, 5] = true;
            this.CustomCellDrawing[8, 6] = true;
            this.CustomCellDrawing[8, 7] = true;
            this.CustomCellDrawing[5, 9] = true;
            this.CustomCellDrawing[5, 10] = true;
            this.CustomCellDrawing[9, 9] = true;
            this.CustomCellDrawing[9, 10] = true;
            this.CustomCellDrawing[6, 11] = true;
            this.CustomCellDrawing[7, 11] = true;
            this.CustomCellDrawing[8, 11] = true;
        }
#endif
    }
}
#endif