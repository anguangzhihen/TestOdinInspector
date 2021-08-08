#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinPrefabUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class OdinPrefabUtility
    {
        public static void UpdatePrefabInstancePropertyModifications(UnityEngine.Object prefabInstance, bool withUndo)
        {
            if (prefabInstance == null) throw new ArgumentNullException("prefabInstance");
            if (!(prefabInstance is ISupportsPrefabSerialization)) throw new ArgumentException("Type must implement ISupportsPrefabSerialization");
            if (!(prefabInstance is ISerializationCallbackReceiver)) throw new ArgumentException("Type must implement ISerializationCallbackReceiver");
            if (!OdinPrefabSerializationEditorUtility.ObjectIsPrefabInstance(prefabInstance)) throw new ArgumentException("Value must be a prefab instance");

            Action action = null;

            EditorApplication.HierarchyWindowItemCallback hierarchyCallback = (arg1, arg2) => action();
            EditorApplication.ProjectWindowItemCallback projectCallback = (arg1, arg2) => action();
            SceneView.OnSceneFunc sceneCallback = (arg) => action();

            EditorApplication.hierarchyWindowItemOnGUI += hierarchyCallback;
            EditorApplication.projectWindowItemOnGUI += projectCallback;
            SceneView.onSceneGUIDelegate += sceneCallback;

            action = () =>
            {
                EditorApplication.hierarchyWindowItemOnGUI -= hierarchyCallback;
                EditorApplication.projectWindowItemOnGUI -= projectCallback;
                SceneView.onSceneGUIDelegate -= sceneCallback;

                // Clear out pre-existing modifications, as they can actually mess this up
                {
                    ISupportsPrefabSerialization supporter = (ISupportsPrefabSerialization)prefabInstance;

                    if (supporter.SerializationData.PrefabModifications != null)
                    {
                        supporter.SerializationData.PrefabModifications.Clear();
                    }

                    if (supporter.SerializationData.PrefabModificationsReferencedUnityObjects != null)
                    {
                        supporter.SerializationData.PrefabModificationsReferencedUnityObjects.Clear();
                    }

                    UnitySerializationUtility.PrefabModificationCache.CachePrefabModifications(prefabInstance, new List<PrefabModification>());
                }

                try
                {
                    if (prefabInstance == null)
                    {
                        // Ignore - the object has been destroyed since the method was invoked.
                        return;
                    }

                    if (Event.current == null) throw new InvalidOperationException("Delayed property modification delegate can only be called during the GUI event loop; Event.current must be accessible.");

                    try
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(prefabInstance);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Exception occurred while calling Unity's PrefabUtility.RecordPrefabInstancePropertyModifications:");
                        Debug.LogException(ex);
                    }

                    var tree = PropertyTree.Create(prefabInstance);

                    tree.DrawMonoScriptObjectField = false;

                    bool isRepaint = Event.current.type == EventType.Repaint;

                    if (!isRepaint)
                    {
                        GUIHelper.PushEventType(EventType.Repaint);
                    }

                    tree.BeginDraw(withUndo);

                    foreach (var property in tree.EnumerateTree(true))
                    {
                        if (property.ValueEntry == null) continue;
                        if (!property.SupportsPrefabModifications) continue;

                        property.Update(true);

                        if (!(property.ChildResolver is IKeyValueMapResolver)) continue;

                        if (property.ValueEntry.DictionaryChangedFromPrefab)
                        {
                            tree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(property, 0);
                        }
                        else
                        {
                            var prefabProperty = tree.PrefabModificationHandler.PrefabPropertyTree.GetPropertyAtPath(property.Path);

                            if (prefabProperty == null) continue;
                            if (prefabProperty.ValueEntry == null) continue;
                            if (!property.SupportsPrefabModifications) continue;
                            if (!(property.ChildResolver is IKeyValueMapResolver)) continue;

                            tree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(property, 0);
                        }
                    }

                    tree.EndDraw();

                    if (!isRepaint)
                    {
                        GUIHelper.PopEventType();
                    }

                    ISerializationCallbackReceiver receiver = (ISerializationCallbackReceiver)prefabInstance;
                    receiver.OnBeforeSerialize();
                    receiver.OnAfterDeserialize();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            };

            foreach (SceneView scene in SceneView.sceneViews)
            {
                scene.Repaint();
            }
        }
    }
}
#endif