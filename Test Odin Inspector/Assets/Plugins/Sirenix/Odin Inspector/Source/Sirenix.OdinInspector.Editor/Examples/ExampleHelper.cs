#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ExampleHelper.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    public static class ExampleHelper
    {
        private static readonly System.Random random = new System.Random();

        private static readonly string[] shaderNames = { "Standard", "Specular", "Skybox/Cubemap" };

        private static readonly string[] strings = { "Hello World", "Sirenix", "Unity", "Lorem Ipsum", "Game Object", "Scriptable Objects", "Ramblings of a mad man" };

        private static readonly string[] meshNames = { "Cube", "Sphere", "Cylinder", "Capsule" };
        
        private static Material[] materials;
        private static Mesh[] meshes;
        private static Texture2D[] textures;

        private static bool initialized;

        private static void InitializeExampleDataSafely()
        {
            if (initialized) return;
            initialized = true;

            materials = shaderNames.Select(s => new Material(Shader.Find(s))).ToArray();
            meshes = meshNames.Select(s => Resources.FindObjectsOfTypeAll<Mesh>().FirstOrDefault(x => x.name == s)).ToArray();
            textures = new Texture2D[]
            {
                EditorIcons.OdinInspectorLogo,
                EditorIcons.UnityLogo,
                (Texture2D)EditorIcons.Upload.Active,
                (Texture2D)EditorIcons.Pause.Active,
                (Texture2D)EditorIcons.Paperclip.Active,
                (Texture2D)EditorIcons.Pen.Active,
                (Texture2D)EditorIcons.Play.Active,
                (Texture2D)EditorIcons.SettingsCog.Active,
                (Texture2D)EditorIcons.ShoppingBasket.Active,
                (Texture2D)EditorIcons.Sound.Active,
            };
        }

        public static T GetScriptableObject<T>(string name) where T: ScriptableObject
        {
            var so = ScriptableObject.CreateInstance<T>();
            so.name = name ?? typeof(T).GetNiceName();
            return so;
        }

        public static Material GetMaterial()
        {
            InitializeExampleDataSafely();
            return PickRandom(materials);
        }

        public static Texture2D GetTexture()
        {
            InitializeExampleDataSafely();
            return PickRandom(textures);
        }

        public static Mesh GetMesh()
        {
            InitializeExampleDataSafely();
            return PickRandom(meshes);
        }

        public static string GetString()
        {
            return PickRandom(strings);
        }

        public static float RandomInt(int min, int max)
        {
            return random.Next(min, max);
        }

        public static float RandomFloat(float min, float max)
        {
            return (float)((random.NextDouble() * (max - min)) + min);
        }

        private static T PickRandom<T>(IList<T> collection)
        {
            return collection[random.Next(collection.Count)];
        }
    }
}
#endif