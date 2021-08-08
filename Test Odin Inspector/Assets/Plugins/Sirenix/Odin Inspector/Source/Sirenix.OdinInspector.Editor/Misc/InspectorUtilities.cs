#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Linq;
    using System.Text;
    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Serialization.Utilities;

    /// <summary>
    /// Provides a variety of miscellaneous utilities widely used in the inspector.
    /// </summary>
    public static class InspectorUtilities
    {
        /// <summary>
        /// Converts an Odin property path to a deep reflection path.
        /// </summary>
        public static string ConvertToDeepReflectionPath(string odinPropertyPath)
        {
            return ConvertOdinPath(odinPropertyPath, isUnity: false);
        }

        /// <summary>
        /// Converts an Odin property path (without groups included) into a Unity property path.
        /// </summary>
        public static string ConvertToUnityPropertyPath(string odinPropertyPath)
        {
            return ConvertOdinPath(odinPropertyPath, isUnity: true);
        }

        private static string ConvertOdinPath(string odinPropertyPath, bool isUnity)
        {
            bool hasSpecialCharacters = false;

            for (int i = 0; i < odinPropertyPath.Length; i++)
            {
                if (odinPropertyPath[i] == '$' || odinPropertyPath[i] == '#')
                {
                    hasSpecialCharacters = true;
                    break;
                }
            }

            if (hasSpecialCharacters)
            {
                using (var sbCache = Cache<StringBuilder>.Claim())
                {
                    StringBuilder sb = sbCache.Value;
                    sb.Length = 0;

                    bool skipUntilNextDot = false;

                    for (int i = 0; i < odinPropertyPath.Length; i++)
                    {
                        var c = odinPropertyPath[i];

                        if (c == '.') skipUntilNextDot = false;
                        else if (skipUntilNextDot) continue;

                        if (c == '$')
                        {
                            sb.Append(isUnity ? "Array.data[" : "[");
                            i++;

                            while (i < odinPropertyPath.Length && char.IsNumber(odinPropertyPath[i]))
                            {
                                sb.Append(odinPropertyPath[i]);
                                i++;
                            }

                            // Insert ']' char after array number
                            sb.Append(']');

                            // Make sure we don't skip the next char
                            i--;
                        }
                        else if (c == '#')
                        {
                            skipUntilNextDot = true;
                            continue;
                        }
                        else if (c == '.')
                        {
                            if (sb.Length > 0 && sb[sb.Length - 1] != '.') // Never add a dot at the start, or just after another dot
                            {
                                sb.Append('.');
                            }
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }

                    while (sb.Length > 0 && sb[0] == '.')
                    {
                        sb.Remove(0, 1);
                    }

                    while (sb.Length > 0 && sb[sb.Length - 1] == '.')
                    {
                        sb.Remove(sb.Length - 1, 1);
                    }

                    return sb.ToString();
                }
            }
            else
            {
                return odinPropertyPath;
            }
        }

        /// <summary>
        /// Prepares a property tree for drawing, and handles management of undo, as well as marking scenes and drawn assets dirty.
        /// </summary>
        /// <param name="tree">The tree to be drawn.</param>
        /// <param name="withUndo">Whether to register undo commands for the changes made to the tree. This can only be set to true if the tree has a <see cref="SerializedObject"/> to represent.</param>
        /// <exception cref="System.ArgumentNullException">tree is null</exception>
#if SIRENIX_INTERNAL
        [Obsolete("Use PropertyTree.BeginDraw instead.", true)]
#else
        [Obsolete("Use PropertyTree.BeginDraw instead.", false)]
#endif
        public static void BeginDrawPropertyTree(PropertyTree tree, bool withUndo)
        {
            tree.BeginDraw(withUndo);
        }

        /// <summary>
        /// Ends drawing a property tree, and handles management of undo, as well as marking scenes and drawn assets dirty.
        /// </summary>
        /// <param name="tree">The tree.</param>
#if SIRENIX_INTERNAL
        [Obsolete("Use PropertyTree.EndDraw instead.", true)]
#else
        [Obsolete("Use PropertyTree.EndDraw instead.", false)]
#endif
        public static void EndDrawPropertyTree(PropertyTree tree)
        {
            tree.EndDraw();
        }

        public static void RegisterUnityObjectDirty(UnityEngine.Object unityObj)
        {
            //var component = unityObj as Component;

            if (AssetDatabase.Contains(unityObj) /*|| (component != null && AssetDatabase.Contains(component.gameObject))*/)
            {
                EditorUtility.SetDirty(unityObj);
                //if (component != null)
                //{
                //    EditorUtility.SetDirty(component.gameObject);
                //}
            }
            else if (Application.isPlaying == false)
            {
                if (unityObj is Component)
                {
                    Component component = (Component)unityObj;
                    EditorUtility.SetDirty(component);
                    EditorUtility.SetDirty(component.gameObject);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                }
                else if (unityObj is EditorWindow || unityObj is ScriptableObject)
                {
                    EditorUtility.SetDirty(unityObj);
                }
                else
                {
                    // We can't find out where this thing is from
                    // It is probably a "temporary" UnityObject created from a script somewhere
                    // Just to be safe, mark it as dirty, and mark all scenes as dirty

                    EditorUtility.SetDirty(unityObj);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }

        /// <summary>
        /// Draws all properties in a given property tree; must be wrapped by a <see cref="BeginDrawPropertyTree(PropertyTree, bool)"/> and <see cref="EndDrawPropertyTree(PropertyTree)"/>.
        /// </summary>
        /// <param name="tree">The tree to be drawn.</param>
        public static void DrawPropertiesInTree(PropertyTree tree)
        {
            tree.DrawProperties();
        }

        /// <summary>
        /// Draws a property in the inspector using a given label.
        /// </summary>
#if SIRENIX_INTERNAL
        [Obsolete("Use InspectorProperty.Draw(label) instead.", true)]
#else
        [Obsolete("Use InspectorProperty.Draw(label) instead.", false)]
#endif
        public static void DrawProperty(InspectorProperty property, GUIContent label)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            property.Draw(label);
        }

    }

    /// <summary>
    /// Odin property system exception.
    /// </summary>
    public class OdinPropertyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of OdinPropertyException.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="innerException">An inner exception.</param>
        public OdinPropertyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
#endif