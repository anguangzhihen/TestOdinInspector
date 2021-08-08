#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeInfoBoxExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using System;

    [AttributeExample(typeof(TypeInfoBoxAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "System", "Sirenix.OdinInspector.Editor.Examples" })]
    internal class TypeInfoBoxExample
    {
        public MyType MyObject = new MyType();

#if UNITY_EDITOR // MyScriptyScriptableObject is an example type and only exists in the editor
        [InfoBox("Click the pen icon to open a new inspector for the Scripty object.")]
        [InlineEditor]
        public MyScriptyScriptableObject Scripty;
#endif

        [Serializable]
        [TypeInfoBox("The TypeInfoBox attribute can be put on type definitions and will result in an InfoBox being drawn at the top of a property.")]
        public class MyType
        {
            public int Value;
        }

        //[TypeInfoBox("The TypeInfoBox attribute can also be used to display a text at the top of, for example, MonoBehaviours or ScriptableObjects.")]
        //public class MyScriptyScriptableObject : ScriptableObject
        //{
        //    public string MyText = ExampleHelper.GetString();
        //    [TextArea(10, 15)]
        //    public string Box;
        //}


#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            Scripty = ExampleHelper.GetScriptableObject<MyScriptyScriptableObject>("Scripty");
        }

        [OnInspectorDispose]
        private void CleanupData()
        {
            if (Scripty != null) UnityEngine.Object.DestroyImmediate(Scripty);
        }
#endif 
    }
}
#endif