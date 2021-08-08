#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TableMatrixExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [ShowOdinSerializedPropertiesInInspector]
    [AttributeExample(typeof(TableMatrixAttribute), "Right-click and drag the column and row labels in order to modify the tables.")]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
    internal class TableMatrixExamples
    {
        [TableMatrix(HorizontalTitle = "Square Celled Matrix", SquareCells = true)]
        public Texture2D[,] SquareCelledMatrix;

        [TableMatrix(SquareCells = true)]
        public Mesh[,] PrefabMatrix;

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            SquareCelledMatrix = new Texture2D[8, 4]
            {
                { ExampleHelper.GetTexture(), null, null, null },
                { null, ExampleHelper.GetTexture(), null, null },
                { null, null, ExampleHelper.GetTexture(), null },
                { null, null, null, ExampleHelper.GetTexture() },
                { ExampleHelper.GetTexture(), null, null, null },
                { null, ExampleHelper.GetTexture(), null, null },
                { null, null, ExampleHelper.GetTexture(), null },
                { null, null, null, ExampleHelper.GetTexture() },
            };

            PrefabMatrix = new Mesh[8, 4]
            {
                { ExampleHelper.GetMesh(), null, null, null },
                { null, ExampleHelper.GetMesh(), null, null },
                { null, null, ExampleHelper.GetMesh(), null },
                { null, null, null, ExampleHelper.GetMesh() },
                { null, null, null, ExampleHelper.GetMesh() },
                { null, null, ExampleHelper.GetMesh(), null },
                { null, ExampleHelper.GetMesh(), null, null },
                { ExampleHelper.GetMesh(), null, null, null },
            };
        }
#endif
    }
}
#endif