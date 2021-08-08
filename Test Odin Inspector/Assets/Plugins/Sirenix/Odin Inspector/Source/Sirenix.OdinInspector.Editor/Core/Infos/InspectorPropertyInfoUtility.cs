#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorPropertyInfoUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Serialization;
    using Sirenix.OdinInspector.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;
    using Utilities;

    public static class InspectorPropertyInfoUtility
    {
        private static readonly Dictionary<Type, bool> TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cache = new Dictionary<Type, bool>(FastTypeComparer.Instance);

        private static readonly HashSet<string> AlwaysSkipUnityProperties = new HashSet<string>()
        {
            "m_PathID",
            "m_FileID",
            "m_ObjectHideFlags",
            "m_PrefabParentObject",
            "m_PrefabInternal",
            "m_PrefabInternal",
            "m_GameObject",
            "m_Enabled",
            "m_Script",
            "m_EditorHideFlags",
            "m_EditorClassIdentifier",
        };

        private static Type System_Object_Type = typeof(object);
        private static Type UnityEngine_Object_Type = typeof(UnityEngine.Object);
        private static Type UnityEngine_Component_Type = typeof(UnityEngine.Component);
        private static Type UnityEngine_MonoBehaviour_Type = typeof(UnityEngine.MonoBehaviour);
        private static Type UnityEngine_Behaviour_Type = typeof(UnityEngine.Behaviour);
        private static Type UnityEngine_ScriptableObject_Type = typeof(UnityEngine.ScriptableObject);

        private static readonly HashSet<string> AlwaysSkipUnityPropertiesForComponents = new HashSet<string>()
        {
            "m_Name",
        };
        
        private static readonly DoubleLookupDictionary<Type, string, string> UnityPropertyMemberNameReplacements = new DoubleLookupDictionary<Type, string, string>()
        {
            { typeof(Bounds), new Dictionary<string, string>() {
                { "m_Extent", "m_Extents" }
            } },
            { typeof(LayerMask), new Dictionary<string, string>() {
                { "m_Bits", "m_Mask" }
            } },
        };

        private static readonly Dictionary<Type, MemberInfo[]> TypeMembers_Cache = new Dictionary<Type, MemberInfo[]>(FastTypeComparer.Instance);

        private static readonly HashSet<Type> NeverProcessUnityPropertiesFor = new HashSet<Type>()
        {
            typeof(Matrix4x4),
            typeof(Color32),
            typeof(AnimationCurve),
            typeof(Gradient),
            typeof(Coroutine)
        };

        private static readonly HashSet<Type> AlwaysSkipUnityPropertiesDeclaredBy = new HashSet<Type>()
        {
            typeof(UnityEngine.Object),
            typeof(ScriptableObject),
            typeof(Component),
            typeof(Behaviour),
            typeof(MonoBehaviour),
            typeof(StateMachineBehaviour),
        };

        /// <summary>
        /// Gets all <see cref="InspectorPropertyInfo" />s for a given type.
        /// </summary>
        /// <param name="parentProperty">The parent property.</param>
        /// <param name="type">The type to get infos for.</param>
        /// <param name="includeSpeciallySerializedMembers">if set to true members that are serialized by Odin will be included.</param>
        public static InspectorPropertyInfo[] GetDefaultPropertiesForType(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return CreateDefaultInspectorProperties(parentProperty, type, includeSpeciallySerializedMembers);

            // Property info caching is no longer viable given the attribute "pre-processor" system

            //InspectorPropertyInfo[] cachedResult;

            //if (PropertyInfoCache.TryGetInnerValue(type, includeSpeciallySerializedMembers, out cachedResult) == false)
            //{
            //    PropertyInfoCache.AddInner(type, includeSpeciallySerializedMembers, cachedResult);
            //    return cachedResult;
            //}
            //else
            //{
            //    var result = new InspectorPropertyInfo[cachedResult.Length];

            //    for (int i = 0; i < result.Length; i++)
            //    {
            //        var copy = cachedResult[i].Copy();
            //        result[i] = copy;

            //        if (copy.HasSingleBackingMember)
            //        {
            //            var attrs = result[i].GetEditableAttributesList();
            //            attrs.Clear();
            //            ProcessAttributes(parentProperty, copy.GetMemberInfo(), attrs);
            //        }
            //    }

            //    return result;
            //}
        }

        private static T Find<T>(this IList<Attribute> attributes) where T : Attribute
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i] is T) return attributes[i] as T;
            }

            return null;
        }

        private static bool Contains<T>(this IList<Attribute> attributes) where T : Attribute
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i] is T) return true;
            }

            return false;
        }

        private static readonly List<Attribute> ChildProcessedAttributes = new List<Attribute>();

        public static bool TryCreate(InspectorProperty parentProperty, MemberInfo member, bool includeSpeciallySerializedMembers, out InspectorPropertyInfo result)
        {
            if (ChildProcessedAttributes.Count > 0)
            {
                ChildProcessedAttributes.Clear();
            }

            var attributes = ChildProcessedAttributes;

            ProcessAttributes(parentProperty, member, attributes);

            bool showInInspector = attributes.Contains<ShowInInspectorAttribute>();

            // Don't make properties for any members marked with or HideInInspector attributes.
            // ShowInInspector attribute overrules HideInInspector attribute.
            if (showInInspector == false && attributes.Contains<HideInInspector>())
            {
                result = null;
                return false;
            }

            // Only show static members if they're marked with the ShowInInspector attribute.
            if (member.IsStatic())
            {
                if (showInInspector)
                {
                    return TryCreate(member, SerializationBackend.None, true, out result, attributes);
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            var parentBackend = GetSerializationBackendOfProperty(parentProperty);
            var actualBackend = GetSerializationBackend(parentProperty, member, parentBackend);
            
            if (showInInspector == false && parentBackend == SerializationBackend.None)
            {
                // If the parent has no backend, we show properties that *would have been* serialized by Odin, if it had an Odin backend.
                var potentialBackend = GetSerializationBackend(parentProperty, member, SerializationBackend.Odin);

                if (potentialBackend != SerializationBackend.None)
                {
                    showInInspector = true;
                }
            }

            if (showInInspector || actualBackend != SerializationBackend.None)
            {
                return TryCreate(member, actualBackend, true, out result, attributes);
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static List<Attribute> CopyList(List<Attribute> attributes)
        {
            var count = attributes.Count;
            var result = new List<Attribute>(count);

            for (int i = 0; i < count; i++)
            {
                result.Add(attributes[i]);
            }

            return result;
        }

        private static bool TryCreate(MemberInfo member, SerializationBackend backend, bool allowEditable, out InspectorPropertyInfo result, List<Attribute> attributes)
        {
            result = null;

            if (member is FieldInfo)
            {
                result = InspectorPropertyInfo.CreateForMember(member, allowEditable, backend, CopyList(attributes));
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo propInfo = member as PropertyInfo;
                PropertyInfo nonAliasedPropInfo = propInfo.DeAliasProperty();

                bool valid = true;

                if (!nonAliasedPropInfo.CanRead || !propInfo.CanRead)
                {
                    valid = false;
                }

                if (valid)
                {
                    result = InspectorPropertyInfo.CreateForMember(member, allowEditable, backend, CopyList(attributes));
                }
            }
            else if (member is MethodInfo)
            {
                var methodInfo = member as MethodInfo;

                if (methodInfo.IsGenericMethodDefinition)
                {
                    return false;
                }

                result = InspectorPropertyInfo.CreateForMember(member, false, SerializationBackend.None, CopyList(attributes));
            }

            if (result != null)
            {
                var orderAttr = result.GetAttribute<PropertyOrderAttribute>();

                if (orderAttr != null)
                {
                    result.Order = orderAttr.Order;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private static int GetMemberCategoryOrder(MemberInfo member)
        {
            if (member == null) return 0; // This tends to happen for aliased Unity properties - pretend they're a field
            if (member is FieldInfo) return 0;
            if (member is PropertyInfo) return 1;
            if (member is MethodInfo) return 2;
            return 3;
        }
        
        /// <summary>
        /// Gets an aliased version of a member, with the declaring type name included in the member name, so that there are no conflicts with private fields and properties with the same name in different classes in the same inheritance hierarchy.
        /// </summary>
        public static MemberInfo GetPrivateMemberAlias(MemberInfo member, string prefixString = null, string separatorString = null)
        {
            if (member is FieldInfo)
            {
                if (separatorString != null)
                {
                    return new MemberAliasFieldInfo(member as FieldInfo, prefixString ?? member.DeclaringType.Name, separatorString);
                }
                else
                {
                    return new MemberAliasFieldInfo(member as FieldInfo, prefixString ?? member.DeclaringType.Name);
                }
            }
            else if (member is PropertyInfo)
            {
                if (separatorString != null)
                {
                    return new MemberAliasPropertyInfo(member as PropertyInfo, prefixString ?? member.DeclaringType.Name, separatorString);
                }
                else
                {
                    return new MemberAliasPropertyInfo(member as PropertyInfo, prefixString ?? member.DeclaringType.Name);
                }
            }
            else if (member is MethodInfo)
            {
                if (separatorString != null)
                {
                    return new MemberAliasMethodInfo(member as MethodInfo, prefixString ?? member.DeclaringType.Name, separatorString);
                }
                else
                {
                    return new MemberAliasMethodInfo(member as MethodInfo, prefixString ?? member.DeclaringType.Name);
                }
            }

            throw new NotImplementedException();
        }

        public static List<InspectorPropertyInfo> CreateMemberProperties(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers)
        {
            List<InspectorPropertyInfo> rootProperties = new List<InspectorPropertyInfo>();

            var assemblyFlag = AssemblyUtilities.GetAssemblyTypeFlag(type.Assembly);

            bool isUnityType = (assemblyFlag == AssemblyTypeFlags.UnityEditorTypes || assemblyFlag == AssemblyTypeFlags.UnityTypes);

            if (isUnityType

                // Unity objects just break in too many cases, with mismatching properties and members.
                && !typeof(UnityEngine.Object).IsAssignableFrom(type) 

                && !NeverProcessUnityPropertiesFor.Contains(type)
                && !(UnityNetworkingUtility.SyncListType != null && type.ImplementsOpenGenericClass(UnityNetworkingUtility.SyncListType))
                && !typeof(UnityAction).IsAssignableFrom(type)
                && !type.ImplementsOpenGenericClass(typeof(UnityAction<>))
                && !type.ImplementsOpenGenericClass(typeof(UnityAction<,>))
                && !type.ImplementsOpenGenericClass(typeof(UnityAction<,,>))
                && !type.ImplementsOpenGenericClass(typeof(UnityAction<,,,>)))
            {
                // It's a Unity type - we do weird stuff for those
                PopulateUnityProperties(parentProperty, type, rootProperties);
            }

            if (rootProperties.Count == 0)
            {
                // With attribute resolvers you can add [ShowInInspector] to non-UnityEngine.Object classes and structs from the UntiyEngine assemblies.
                // But those types are drawn throught EditorGUILayout.PropertyField(serializedProperty)
                // The above PopulateUnityProperties, uses the SerializeObject to find out which properties to show, tbuat that doesn't take Odin's [ShowInInspector] attribtues into account.
                // So instead of messing with that code, we've made a sort of fallback, where if the PopulateUnityProperties doesn't find ANY properties,
                // Then we'll try Odin's default way of doing it.
                // The best solution would be if we didn't use the SerializedObject to determaine which members to include, but don't know what would go wrong if we tried to do that.
                // What to do....

                PopulateMemberInspectorProperties(parentProperty, type, includeSpeciallySerializedMembers, rootProperties);
            }

            rootProperties = rootProperties.OrderBy(n => n.Order)
                                 .ThenBy(n => GetMemberCategoryOrder(n.GetMemberInfo()))
                                 .ToList();
            return rootProperties;
        }

        public static InspectorPropertyInfo[] PerformAndBakePostGroupOrdering(List<InspectorPropertyInfo> rootProperties, Dictionary<InspectorPropertyInfo, float> groupMemberOrdering = null)
        {
            var result = rootProperties.OrderBy(n =>
            {
                if (n.PropertyType == PropertyType.Group && n.Order == 0)
                {
                    return FindFirstMemberOfGroup(n).Order;
                }

                return n.Order;
            });

            if (groupMemberOrdering != null)
            {
                result = result.ThenBy(n => groupMemberOrdering[n]);
            }

            return result.ThenBy(n => GetMemberCategoryOrder(n.GetMemberInfo()))
                         //.Examine(n => Debug.Log("ROOT: " + n + " --- (" + n.Order + ", " + groupMemberOrdering[n] + ", " + GetMemberCategoryOrder(n.GetMemberInfo()) + ")"))
                         .ToArray();
        }

        private static InspectorPropertyInfo[] CreateDefaultInspectorProperties(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers)
        {
            List<InspectorPropertyInfo> rootProperties = CreateMemberProperties(parentProperty, type, includeSpeciallySerializedMembers);

            Dictionary<InspectorPropertyInfo, float> groupMemberOrders = GroupMemberOrders_Cached;

            BuildPropertyGroups(parentProperty, type, rootProperties, includeSpeciallySerializedMembers, ref groupMemberOrders);

            var result = PerformAndBakePostGroupOrdering(rootProperties, groupMemberOrders);

            if (groupMemberOrders.Count > 0)
                groupMemberOrders.Clear();

            return result;
        }

        private static InspectorPropertyInfo FindFirstMemberOfGroup(InspectorPropertyInfo groupInfo)
        {
            for (int i = 0; i < groupInfo.GetGroupInfos().Length; i++)
            {
                var info = groupInfo.GetGroupInfos()[i];

                if (info.PropertyType == PropertyType.Group)
                {
                    var result = FindFirstMemberOfGroup(info);

                    if (result != null)
                    {
                        return result;
                    }
                }
                else
                {
                    return info;
                }
            }

            return null;
        }

        private static readonly Dictionary<InspectorPropertyInfo, float> GroupMemberOrders_Cached = new Dictionary<InspectorPropertyInfo, float>();
        private static readonly Dictionary<string, GroupData> GroupTree_Cached = new Dictionary<string, GroupData>();
        private static readonly Dictionary<InspectorPropertyInfo, InspectorPropertyInfo> RemovedMembers_Cached = new Dictionary<InspectorPropertyInfo, InspectorPropertyInfo>();

        public static InspectorPropertyInfo[] BuildPropertyGroupsAndFinalize(InspectorProperty parentProperty, Type typeOfOwner, List<InspectorPropertyInfo> rootMemberProperties, bool includeSpeciallySerializedMembers)
        {
            for (int i = 0; i < rootMemberProperties.Count; i++)
            {
                rootMemberProperties[i].UpdateOrderFromAttributes();
            }

            var groupMemberOrders = GroupMemberOrders_Cached;

            BuildPropertyGroups(parentProperty, typeOfOwner, rootMemberProperties, includeSpeciallySerializedMembers, ref groupMemberOrders);
            var result = PerformAndBakePostGroupOrdering(rootMemberProperties, groupMemberOrders);

            if (groupMemberOrders.Count > 0)
                groupMemberOrders.Clear();

            return result;
        }

        private struct GroupDataAndInfo
        {
            public GroupData Data;
            public InspectorPropertyInfo Info;
        }

        public static void BuildPropertyGroups(InspectorProperty parentProperty, Type typeOfOwner, List<InspectorPropertyInfo> rootMemberProperties, bool includeSpeciallySerializedMembers, ref Dictionary<InspectorPropertyInfo, float> groupMemberOrders)
        {
            if (rootMemberProperties.Count == 0)
                return;

            if (groupMemberOrders == null)
                groupMemberOrders = new Dictionary<InspectorPropertyInfo, float>(rootMemberProperties.Count);
            else if (groupMemberOrders.Count > 0)
                groupMemberOrders.Clear();

            for (int i = 0; i < rootMemberProperties.Count; i++)
            {
                groupMemberOrders.Add(rootMemberProperties[i], i);
            }

            // For lambda references
            var lambdaRefMemberOrders = groupMemberOrders;

            Dictionary<string, GroupData> groupTree = GroupTree_Cached;

            if (groupTree.Count > 0)
                groupTree.Clear();

            // Build group tree
            {
                //rootMemberProperties.ForEach(member => member.GetAttributes<PropertyGroupAttribute>().ForEach(attr => RegisterGroupAttribute(member, attr, groupTree)));

                for (int i = 0; i < rootMemberProperties.Count; i++)
                {
                    var propInfo = rootMemberProperties[i];
                    var attributes = propInfo.Attributes;

                    for (int j = 0; j < attributes.Count; j++)
                    {
                        var attr = attributes[j] as PropertyGroupAttribute;

                        if (attr != null)
                        {
                            RegisterGroupAttribute(propInfo, attr, groupTree);
                        }
                    }
                }
            }

            // Validate group tree, cull invalid groups and consolidate group attributes
            {
                //groupTree = groupTree.Where(n => ProcessGroups(n.Value, groupTree))
                //                     .ToDictionary(n => n.Key, n => n.Value);

                var toRemove = new List<string>();

                foreach (var entry in groupTree)
                {
                    if (!ProcessGroups(entry.Value, groupTree))
                    {
                        toRemove.Add(entry.Key);
                    }
                }

                for (int i = 0; i < toRemove.Count; i++)
                {
                    groupTree.Remove(toRemove[i]);
                }
            }

            List<GroupDataAndInfo> groups = new List<GroupDataAndInfo>();

            // Create groups from group tree
            {
                //var groups = groupTree.Values.Select(n => new { Data = n, Group = CreatePropertyGroups(parentProperty, typeOfOwner, n, lambdaRefMemberOrders, includeSpeciallySerializedMembers) }).ToList();

                foreach (var groupData in groupTree.Values)
                {
                    var info = CreatePropertyGroups(parentProperty, typeOfOwner, groupData, lambdaRefMemberOrders, includeSpeciallySerializedMembers);

                    groups.Add(new GroupDataAndInfo()
                    {
                        Data = groupData,
                        Info = info
                    });
                }
            }

            if (groupTree.Count > 0)
                groupTree.Clear();

            Dictionary<InspectorPropertyInfo, InspectorPropertyInfo> removedMembers = RemovedMembers_Cached;

            if (removedMembers.Count > 0)
                removedMembers.Clear();

            // Replace root level members with groups
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var members = RecurseGroupMembers(group.Data)
                                .OrderBy(n => lambdaRefMemberOrders[n]);

                var firstMember = members.First();
                var index = rootMemberProperties.IndexOf(firstMember);

                // Check for aliasing
                {
                    string finalGroupName = "#" + group.Data.Name;

                    var hiddenPropertyIndex = rootMemberProperties.FindIndex(n => n.PropertyName == finalGroupName);

                    if (hiddenPropertyIndex >= 0)
                    {
                        var hiddenProperty = rootMemberProperties[hiddenPropertyIndex];

                        // We need to alias either a group or a member
                        InspectorPropertyInfo newAliasForHiddenProperty;

                        if (TryHidePropertyWithGroup(parentProperty, hiddenProperty, group.Info, includeSpeciallySerializedMembers, out newAliasForHiddenProperty))
                        {
                            rootMemberProperties[hiddenPropertyIndex] = newAliasForHiddenProperty;
                            removedMembers[hiddenProperty] = group.Info;
                            groupMemberOrders[newAliasForHiddenProperty] = groupMemberOrders[hiddenProperty];
                        }
                    }
                }

                if (index >= 0)
                {
                    //Debug.Log("REPLACE " + firstMember.PropertyName + " WITH GROUP " + group.Data.ID);

                    removedMembers.Add(rootMemberProperties[index], group.Info);
                    groupMemberOrders[group.Info] = groupMemberOrders[rootMemberProperties[index]];
                    rootMemberProperties[index] = group.Info;
                }
                else
                {
                    var removedByGroup = removedMembers[firstMember];

                    index = rootMemberProperties.IndexOf(removedByGroup);

                    rootMemberProperties.Insert(index + 1, group.Info);
                    groupMemberOrders[group.Info] = groupMemberOrders[rootMemberProperties[index]] + 0.1f;
                }
            }

            // Remove all remaining root members that are contained in any groups
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var members = RecurseGroupMembers(group.Data);

                foreach (var member in members)
                {
                    if (!removedMembers.ContainsKey(member))
                    {
                        removedMembers.Add(member, group.Info);
                    }

                    rootMemberProperties.Remove(member);
                }
            }

            if (removedMembers.Count > 0)
                removedMembers.Clear();
        }

        private static void RegisterGroupAttribute(InspectorPropertyInfo member, PropertyGroupAttribute attribute, Dictionary<string, GroupData> groupTree)
        {
            string[] path = attribute.GroupID.Split('/');

            string firstPathStep = path[0];

            GroupData currentGroup;

            if (!groupTree.TryGetValue(firstPathStep, out currentGroup))
            {
                currentGroup = new GroupData();
                currentGroup.ID = firstPathStep;
                currentGroup.Name = firstPathStep;

                groupTree.Add(firstPathStep, currentGroup);
            }

            for (int i = 1; i < path.Length; i++)
            {
                string step = path[i];

                var nextGroup = currentGroup.ChildGroups.FirstOrDefault(n => n.Name == step);

                if (nextGroup == null)
                {
                    nextGroup = new GroupData();
                    nextGroup.ID = string.Join("/", path.Take(i + 1).ToArray());
                    nextGroup.Name = step;
                    nextGroup.Parent = currentGroup;

                    currentGroup.ChildGroups.Add(nextGroup);
                }

                currentGroup = nextGroup;
            }

            var info = new GroupAttributeInfo();

            info.InspectorPropertyInfo = member;
            info.Attribute = attribute;

            currentGroup.Attributes.Add(info);
        }

        private static bool ProcessGroups(GroupData groupData, Dictionary<string, GroupData> groupTree)
        {
            if (groupData.Attributes.Count == 0)
            {
                foreach (var expectingGroup in RecurseGroups(groupData).Where(n => n.Attributes.Count > 0))
                {
                    foreach (var attrInfo in expectingGroup.Attributes)
                    {
                        Debug.LogError(
                            "Group attribute '" + attrInfo.Attribute.GetType().Name +
                            "' on member '" + attrInfo.InspectorPropertyInfo.PropertyName +
                            "' expected a group with the name '" + groupData.Name +
                            "' to exist in declaring type '" +
                            attrInfo.InspectorPropertyInfo.TypeOfOwner.GetNiceName() +
                            "'. Its ID was '" + expectingGroup.ID + "'."
                        );
                    }
                }

                return false;
            }

            var groupName = groupData.Name;

            for (int i = 0; i < groupName.Length; i++)
            {
                if (groupName[i] == '.')
                {
                    Debug.LogError("Group name '" + groupData.Name + "' is invalid; group names or paths cannot contain '.'!");
                    return false;
                }
            }

            // Consolidate the various group attributes into a single group attribute
            {
                // @Performance
                // Note that we MUST make a deep-cloned copy of the attribute here. This is a bit of a performance sink,
                //  but is absolutely necessary to avoid caching-based errors; in short, we are not allowed to mutate
                //  attributes that are passed into the grouping system.
                //
                // If any would-be optimizers feel like taking this on, one option would be to build a fast-deep-clone
                //  system that is not serialization-based. This would also be useful other places in the code-base,
                //  but doing it well enough to be worth it is not a small endeavour.
                //
                // Another idea is to check for a cloneable interface (such as ICloneable, though a custom 
                //  IDeepCloneable might be better) and invoke that. It could easily be implemented on all built-in
                //  Odin group attributes which should make for decent performance in most cases.

                groupData.ConsolidatedAttribute = (PropertyGroupAttribute)SerializationUtility.CreateCopy(groupData.Attributes[0].Attribute);

                Type groupAttrType = groupData.ConsolidatedAttribute.GetType();

                for (int i = 1; i < groupData.Attributes.Count; i++)
                {
                    var attrInfo = groupData.Attributes[i];

                    if (attrInfo.Attribute.GetType() != groupAttrType)
                    {
                        Debug.LogError(
                            "Cannot have group attributes of different types with the " +
                            "same group name, on the same type (or its inherited types): " +
                            "Group type mismatch: the group '" + groupData.ID
                            + "' is expecting attributes of type '" + groupAttrType.Name +
                            "', but got an attribute of type '" + attrInfo.Attribute.GetType().Name +
                            "' on the property '" + attrInfo.InspectorPropertyInfo.TypeOfOwner.GetNiceName() +
                            "." + attrInfo.InspectorPropertyInfo.PropertyName + "'.");

                        groupData.Attributes.RemoveAt(i--);
                        continue;
                    }
                    else
                    {
                        // Consolidate attribute
                        try
                        {
                            groupData.ConsolidatedAttribute = groupData.ConsolidatedAttribute.Combine(attrInfo.Attribute);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }

            // Add subgroups if applicable
            {
                ISubGroupProviderAttribute subGroupProvider = groupData.ConsolidatedAttribute as ISubGroupProviderAttribute;

                if (subGroupProvider != null)
                {
                    string[] groupPath = groupData.ID.Split('/');

                    Dictionary<string, PropertyGroupAttribute> subGroupPaths = new Dictionary<string, PropertyGroupAttribute>();

                    foreach (var subGroupAttribute in subGroupProvider.GetSubGroupAttributes())
                    {
                        string[] subGroupPath = subGroupAttribute.GroupID.Split('/');

                        bool valid = true;

                        if (subGroupPath.Length != groupPath.Length + 1)
                        {
                            valid = false;
                        }

                        if (valid)
                        {
                            for (int i = 0; i < groupPath.Length; i++)
                            {
                                if (subGroupPath[i] != groupPath[i])
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }

                        if (valid)
                        {
                            var subGroupData = groupData.ChildGroups.FirstOrDefault(n => n.Name == subGroupAttribute.GroupName);

                            if (subGroupData == null)
                            {
                                subGroupData = new GroupData();

                                subGroupData.ID = subGroupAttribute.GroupID;
                                subGroupData.Name = subGroupAttribute.GroupName;
                                subGroupData.Parent = groupData;

                                groupData.ChildGroups.Add(subGroupData);
                            }

                            if (!subGroupPaths.ContainsKey(subGroupAttribute.GroupID))
                            {
                                subGroupPaths.Add(subGroupAttribute.GroupID, subGroupAttribute);
                            }

                            var attrInfo = new GroupAttributeInfo();

                            attrInfo.InspectorPropertyInfo = groupData.Attributes[0].InspectorPropertyInfo;
                            attrInfo.Attribute = subGroupAttribute;
                            attrInfo.Exclude = true;

                            subGroupData.Attributes.Add(attrInfo);
                        }
                        else
                        {
                            Debug.LogError("Subgroup '" + subGroupAttribute.GroupID + "' of type '" + subGroupAttribute.GetType().Name + "' for group '" + groupData.ID + "' of type '" + groupData.ConsolidatedAttribute.GetType().Name + "' must have an ID that starts with '" + groupData.ID + "' and continue one path step further.");
                        }
                    }

                    for (int i = 0; i < groupData.Attributes.Count; i++)
                    {
                        var attrInfo = groupData.Attributes[i];
                        var newPath = subGroupProvider.RepathMemberAttribute(attrInfo.Attribute);

                        if (newPath != null && newPath != attrInfo.Attribute.GroupID)
                        {
                            if (!subGroupPaths.ContainsKey(newPath))
                            {
                                Debug.LogError("Member '" + attrInfo.InspectorPropertyInfo.PropertyName + "' of " + groupData.ConsolidatedAttribute.GetType().Name + " group '" + groupData.ID + "' was repathed to subgroup at path '" + newPath + "', but no such subgroup was defined.");
                                continue;
                            }

                            groupData.Attributes.RemoveAt(i--);

                            attrInfo.Attribute = subGroupPaths[newPath];

                            var subGroup = groupData.ChildGroups.First(n => n.ID == newPath);
                            subGroup.Attributes.Add(attrInfo);
                        }
                    }
                }
            }

            // Recurse on children and remove invalid children
            //groupData.ChildGroups.RemoveAll(child => );

            for (int i = 0; i < groupData.ChildGroups.Count; i++)
            {
                if (!ProcessGroups(groupData.ChildGroups[i], groupTree))
                {
                    groupData.ChildGroups.RemoveAt(i);
                    i--;
                }
            }

            // Remove duplicate group members if applicable
            {
                HashSet<string> memberNames = new HashSet<string>();

                for (int i = 0; i < groupData.Attributes.Count; i++)
                {
                    var attrInfo = groupData.Attributes[i];

                    if (attrInfo.InspectorPropertyInfo.PropertyType == PropertyType.Group || attrInfo.Exclude) continue;

                    var name = attrInfo.InspectorPropertyInfo.PropertyName;

                    if (!memberNames.Add(name))
                    {
                        groupData.Attributes.RemoveAt(i--);
                    }
                }
            }

            //groupData.Attributes.Sort((a, b) => a.InspectorPropertyInfo.Order.CompareTo(b.InspectorPropertyInfo.Order));

            return true;
        }

        private static InspectorPropertyInfo CreatePropertyGroups(InspectorProperty parentProperty, Type typeOfOwner, GroupData groupData, Dictionary<InspectorPropertyInfo, float> memberOrder, bool includeSpeciallySerializedMembers)
        {
            List<InspectorPropertyInfo> children = new List<InspectorPropertyInfo>();

            foreach (var attrInfo in groupData.Attributes)
            {
                if (attrInfo.Exclude) continue;

                children.Add(attrInfo.InspectorPropertyInfo);
            }

            // Replace members with sub groups
            foreach (var childGroupData in groupData.ChildGroups)
            {
                var childGroup = CreatePropertyGroups(parentProperty, typeOfOwner, childGroupData, memberOrder, includeSpeciallySerializedMembers);

                // Insert child group where the first member in said group would have been
                InspectorPropertyInfo firstMember = null;
                float currentMinFloatOrder = 0;

                foreach (var member in RecurseGroupMembers(childGroupData))
                {
                    var floatOrderValue = memberOrder[member];

                    if (firstMember == null || floatOrderValue < currentMinFloatOrder)
                    {
                        currentMinFloatOrder = floatOrderValue;
                        firstMember = member;
                    }
                }

                var index = children.IndexOf(firstMember);

                if (index >= 0)
                {
                    //Debug.Log("REPLACE " + firstMember.PropertyName + " WITH GROUP " + childGroup.GetAttribute<PropertyGroupAttribute>().GroupID);

                    memberOrder[childGroup] = memberOrder[children[index]];
                    children[index] = childGroup;
                }
                else
                {
                    memberOrder[childGroup] = memberOrder[firstMember];
                    children.Insert(0, childGroup);
                }

                // Hide any aliased properties
                string finalGroupName = "#" + childGroup.PropertyName;

                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];

                    if (child != childGroup && child.PropertyName == finalGroupName)
                    {
                        InspectorPropertyInfo newAliasForHiddenProperty;

                        if (TryHidePropertyWithGroup(parentProperty, child, childGroup, includeSpeciallySerializedMembers, out newAliasForHiddenProperty))
                        {
                            memberOrder[newAliasForHiddenProperty] = memberOrder[children[i]];
                            children[i] = newAliasForHiddenProperty;
                        }
                    }
                }
            }

            // Remove the rest of the members
            foreach (var childGroupData in groupData.ChildGroups)
            {
                var members = RecurseGroupMembers(childGroupData);

                foreach (var member in members)
                {
                    children.Remove(member);
                }
            }

            //children = children.OrderBy(n => n.Order)
            //                   .ThenBy(n => memberOrder[n])
            //                   .ThenBy(n => GetMemberCategoryOrder(n.GetMemberInfo()))
            //                   //.Examine(n => Debug.Log(groupData.ID + ": " + n + " --- (" + n.Order + ", " + memberOrder[n] + ", " + GetMemberCategoryOrder(n.MemberInfo) + ")"))
            //                   .ToList();

            children.Sort((a, b) =>
            {
                int compare = a.Order.CompareTo(b.Order);
                if (compare != 0) return compare;
                compare = memberOrder[a].CompareTo(memberOrder[b]);
                if (compare != 0) return compare;
                compare = GetMemberCategoryOrder(a.GetMemberInfo()).CompareTo(GetMemberCategoryOrder(b.GetMemberInfo()));
                return compare;
            });

            float order = groupData.ConsolidatedAttribute.Order;

            if (order == 0)
            {
                //order = RecurseGroupMembers(groupData).Min(n => n.Order);

                order = float.MaxValue; 

                foreach (var member in RecurseGroupMembers(groupData))
                {
                    if (member.Order < order)
                    {
                        order = member.Order;
                    }
                }
                
            }

            var result = InspectorPropertyInfo.CreateGroup("#" + groupData.Name, typeOfOwner, order, children.ToArray(), new List<Attribute>() { groupData.ConsolidatedAttribute });
            return result;
        }

        private static bool TryHidePropertyWithGroup(InspectorProperty parentProperty, InspectorPropertyInfo hidden, InspectorPropertyInfo group, bool includeSpeciallySerializedMembers, out InspectorPropertyInfo newAliasForHiddenProperty)
        {
            if (hidden.PropertyType == PropertyType.Group)
            {
                var newGroupName = group.TypeOfOwner.GetNiceName() + "." + group.PropertyName;
                var oldGroupName = hidden.TypeOfOwner.GetNiceName() + "." + hidden.PropertyName;

                Debug.LogWarning("Property group '" + newGroupName + "' conflicts with already existing group property '" + oldGroupName + "'. Group property '" + newGroupName + "' will be removed from the property tree.");
                newAliasForHiddenProperty = null;
                return false;
            }
            else if (hidden.GetMemberInfo() != null)
            {
                var alias = GetPrivateMemberAlias(hidden.GetMemberInfo(), hidden.TypeOfOwner.GetNiceName(), " -> ");

                var aliasName = alias.Name;
                var groupName = group.TypeOfOwner.GetNiceName() + "." + group.PropertyName;
                var hiddenPropertyName = hidden.TypeOfOwner.GetNiceName() + "." + hidden.PropertyName;

                if (InspectorPropertyInfoUtility.TryCreate(parentProperty, alias, includeSpeciallySerializedMembers, out newAliasForHiddenProperty))
                {
                    Debug.LogWarning("Property group '" + groupName + "' hides member property '" + hiddenPropertyName + "'. Alias property '" + aliasName + "' created for member property '" + hiddenPropertyName + "'.");
                    return true;
                }
                else
                {
                    Debug.LogWarning("Property group '" + groupName + "' tries to hide member property '" + hiddenPropertyName + "', but failed to create alias property '" + aliasName + "' for member property '" + hiddenPropertyName + "'; group property '" + groupName + "' will be removed.");
                    return false;
                }
            }

            newAliasForHiddenProperty = null;
            return false;
        }

        private static IEnumerable<GroupData> RecurseGroups(GroupData groupData)
        {
            yield return groupData;

            for (int i = 0; i < groupData.ChildGroups.Count; i++)
            {
                var childGroup = groupData.ChildGroups[i];

                foreach (var child in RecurseGroups(childGroup))
                {
                    yield return child;
                }
            }

            //foreach (var child in groupData.ChildGroups.SelectMany(n => RecurseGroups(n)))
            //{
            //    yield return child;
            //}
        }

        private static IEnumerable<InspectorPropertyInfo> RecurseGroupMembers(GroupData groupData)
        {
            for (int i = 0; i < groupData.Attributes.Count; i++)
            {
                GroupAttributeInfo attrInfo = groupData.Attributes[i];
                yield return attrInfo.InspectorPropertyInfo;
            }

            for (int i = 0; i < groupData.ChildGroups.Count; i++)
            {
                var childGroup = groupData.ChildGroups[i];

                foreach (var child in RecurseGroups(childGroup))
                {
                    for (int j = 0; j < child.Attributes.Count; j++)
                    {
                        yield return child.Attributes[j].InspectorPropertyInfo;
                    }
                }
            }

            //foreach (var childGroup in groupData.ChildGroups.SelectMany(n => RecurseGroups(n)))
            //{
            //    foreach (var attrInfo in childGroup.Attributes)
            //    {
            //        yield return attrInfo.InspectorPropertyInfo;
            //    }
            //}
        }

        private static Dictionary<SerializationBackend, Dictionary<Type, List<InspectorPropertyInfo>>> UnityPropertyInfoCache = new Dictionary<SerializationBackend, Dictionary<Type, List<InspectorPropertyInfo>>>();

        private static void PopulateUnityProperties(InspectorProperty parentProperty, Type type, List<InspectorPropertyInfo> result)
        {
            Dictionary<Type, List<InspectorPropertyInfo>> innerDict;
            var parentBackend = GetSerializationBackendOfProperty(parentProperty);

            if (!UnityPropertyInfoCache.TryGetValue(parentBackend, out innerDict))
            {
                innerDict = new Dictionary<Type, List<InspectorPropertyInfo>>(FastTypeComparer.Instance);
                UnityPropertyInfoCache.Add(parentBackend, innerDict);
            }

            List<InspectorPropertyInfo> unityProperties;
            if (!innerDict.TryGetValue(type, out unityProperties))
            {
                unityProperties = new List<InspectorPropertyInfo>();
                FindUnityProperties(parentProperty, type, unityProperties);
                innerDict.Add(type, unityProperties);
            }

            var count = unityProperties.Count;

            for (int i = 0; i < count; i++)
            {
                var copy = unityProperties[i].CreateCopy();
                result.Add(copy);
            }
        }

        private static void FindUnityProperties(InspectorProperty parentProperty, Type type, List<InspectorPropertyInfo> result)
        { 
            // Steal the properties from Unity; we have no way of knowing what Unity is going to do with this type
            SerializedProperty prop;

            if (type.IsAbstract || type.IsInterface || type.IsArray) return;

            UnityEngine.Object toDestroy = null;

            if (typeof(Component).IsAssignableFrom(type))
            {
                GameObject go = new GameObject("temp");
                Component component;

                if (type.IsAssignableFrom(typeof(Transform)))
                {
                    component = go.transform;
                }
                else
                {
                    component = go.AddComponent(type);
                }

                SerializedObject obj = new SerializedObject(component);
                prop = obj.GetIterator();

                toDestroy = go;
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                ScriptableObject scriptableObject = ScriptableObject.CreateInstance(type);

                SerializedObject obj = new SerializedObject(scriptableObject);
                prop = obj.GetIterator();

                toDestroy = scriptableObject;
            }
            else if (UnityVersion.IsVersionOrGreater(2017, 1))
            {
                // Unity broke creation of emitted scriptable objects in 2017.1, but emitting
                // MonoBehaviours still works.

                GameObject go = new GameObject();
                var handle = UnityPropertyEmitter.CreateEmittedMonoBehaviourProperty("InspectorPropertyInfo_UnityPropertyExtractor", type, 1, ref go);
                prop = handle.UnityProperty;
                toDestroy = go;
            }
            else
            {
                prop = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty("InspectorPropertyInfo_UnityPropertyExtractor", type, 1);

                if (prop != null)
                {
                    toDestroy = prop.serializedObject.targetObject;
                }
            }

            try
            {
                if (prop == null)
                {
                    //Debug.LogWarning("Could not get serialized property for type " + type.GetNiceName() + "; this type will not be shown in the inspector.");
                    return;
                }

                //// Occasionally used debug code to inspect all serialized properties for types
                //{
                //    string path = prop.propertyPath;
                //    if (prop.Next(true))
                //    {
                //        do
                //        {
                //            Debug.Log(type.GetNiceFullName() + "." + prop.propertyPath + " - " + prop.propertyType);
                //        } while (prop.Next(true));

                //        prop = prop.serializedObject.FindProperty(path);
                //    }
                //}

                // Enter children if there are any
                if (prop.Next(true))
                {
                    var members = type.GetAllMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                      .Where(n => (n is FieldInfo || n is PropertyInfo) && !AlwaysSkipUnityPropertiesDeclaredBy.Contains(n.DeclaringType))
                                      .ToList();

                    // Iterate through children (but not sub-children)
                    do
                    {
                        if (AlwaysSkipUnityProperties.Contains(prop.name)) continue;
                        if (typeof(Component).IsAssignableFrom(type) && AlwaysSkipUnityPropertiesForComponents.Contains(prop.name)) continue;

                        string memberName = prop.name;

                        if (UnityPropertyMemberNameReplacements.ContainsKeys(type, memberName))
                        {
                            memberName = UnityPropertyMemberNameReplacements[type][memberName];
                        }

                        MemberInfo member = members.FirstOrDefault(n => n.Name == memberName || n.Name == prop.name);

                        if (member == null)
                        {
                            // Try to find a member that matches the display name
                            var propName = prop.displayName.Replace(" ", "");
                            bool changedPropName = false;

                            if (string.Equals(propName, "material", StringComparison.InvariantCultureIgnoreCase))
                            {
                                changedPropName = true;
                                propName = "sharedMaterial";
                            }
                            else if (string.Equals(propName, "mesh", StringComparison.InvariantCultureIgnoreCase))
                            {
                                changedPropName = true;
                                propName = "sharedMesh";
                            }

                            member = members.FirstOrDefault(n => string.Equals(n.Name, propName, StringComparison.InvariantCultureIgnoreCase) && prop.IsCompatibleWithType(n.GetReturnType()));

                            if (changedPropName && member == null)
                            {
                                // Try again with the old name
                                propName = prop.displayName.Replace(" ", "");
                                member = members.FirstOrDefault(n => string.Equals(n.Name, propName, StringComparison.InvariantCultureIgnoreCase) && prop.IsCompatibleWithType(n.GetReturnType()));
                            }
                        }

                        if (member == null)
                        {
                            // Now we are truly getting desperate.
                            // Look away, kids - this code is rated M for Monstrous

                            var propName = prop.displayName;
                            //string typeName = prop.GetProperTypeName();

                            var possibles = members.Where(n => (propName.Contains(n.Name, StringComparison.InvariantCultureIgnoreCase) || n.Name.Contains(propName, StringComparison.InvariantCultureIgnoreCase)) && prop.IsCompatibleWithType(n.GetReturnType())).ToList();

                            if (possibles.Count == 1)
                            {
                                // We found only one possibly compatible member
                                // It's... *probably* this one
                                member = possibles[0];
                            }
                        }

                        if (member == null)
                        {
                            // If we can alias this Unity property as a "virtual member", do that
                            var valueType = prop.GuessContainedType();

                            if (valueType != null && SerializedPropertyUtilities.CanSetGetValue(valueType))
                            {
                                result.Add(InspectorPropertyInfo.CreateForUnityProperty(prop.name, type, valueType, prop.editable, null));
                                continue;
                            }
                        }

                        if (member == null)
                        {
                            // Suppress warning, this generally just doesn't and shouldn't work - just sometimes there could be such a member in theory, so now at least we looked
                            if (prop.name == "Array" && prop.propertyType == SerializedPropertyType.Generic)
                                continue;

                            Debug.LogWarning("Failed to find corresponding member for Unity property '" + prop.name + "/" + prop.displayName + "' on type " + type.GetNiceName() + ", and cannot alias a Unity property of type '" + prop.propertyType + "/" + prop.type + "'. This property will be missing in the inspector.");
                            continue;
                        }

                        // Makes things easier if we can only find the same member once
                        members.Remove(member);

                        InspectorPropertyInfo info;

                        // Add Unity's found property member as an info
                        List<Attribute> attributes = new List<Attribute>();
                        ProcessAttributes(parentProperty, member, attributes);

                        if (TryCreate(member, GetSerializationBackend(parentProperty, member), prop.editable, out info, attributes))
                        {
                            // Make sure the names match - that way, we can find the property again
                            // when we create a Unity property path from the names
                            info.PropertyName = prop.name;

                            result.Add(info);
                        }
                    } while (prop.Next(false));
                }
            }
            catch (InvalidOperationException)
            {
                // Ignore; it just means we've reached the end of the property
            }
            finally
            {
                if (toDestroy != null)
                {
                    UnityEngine.Object.DestroyImmediate(toDestroy);
                }
            }
        }

        private static void PopulateMemberInspectorProperties(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers, List<InspectorPropertyInfo> properties)
        {
            if (type.IsPrimitive || type == typeof(string))
            {
                // Don't go through the members of primitives or strings.
                return;
            }

            var baseType = type.BaseType;

            if (!object.ReferenceEquals(baseType, null) &&
                !object.ReferenceEquals(baseType, System_Object_Type) &&
                !object.ReferenceEquals(baseType, UnityEngine_Object_Type) &&
                !object.ReferenceEquals(baseType, UnityEngine_Component_Type) &&
                !object.ReferenceEquals(baseType, UnityEngine_MonoBehaviour_Type) &&
                !object.ReferenceEquals(baseType, UnityEngine_Behaviour_Type) &&
                !object.ReferenceEquals(baseType, UnityEngine_ScriptableObject_Type))
            {
                PopulateMemberInspectorProperties(parentProperty, baseType, includeSpeciallySerializedMembers, properties);
            }

            MemberInfo[] members;

            if (!TypeMembers_Cache.TryGetValue(type, out members))
            {
                members = type.GetMembers(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                TypeMembers_Cache.Add(type, members);
            }

            //if (members.Length > 100)
            //{
            //    Debug.Log("What the fuck, this thing had >100 members: " + type.GetNiceFullName() + " - Path: " + parentProperty.Path + " - Root: " + parentProperty.Tree.TargetType.GetNiceFullName() + " - Base Value: " + parentProperty.ValueEntry.TypeOfValue.GetNiceFullName());
            //}

            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                if (!(member is FieldInfo || member is PropertyInfo || member is MethodInfo)) continue;

                if (member is PropertyInfo)
                {
                    var pi = member as PropertyInfo;

                    var parameters = pi.GetIndexParameters();

                    // We cannot handle properties with index parameters
                    if (parameters.Length > 0)
                        continue;
                }

                InspectorPropertyInfo info;

                if (InspectorPropertyInfoUtility.TryCreate(parentProperty, member, includeSpeciallySerializedMembers, out info))
                {
                    InspectorPropertyInfo previousPropertyWithName = null;
                    int previousPropertyIndex = -1;

                    for (int j = 0; j < properties.Count; j++)
                    {
                        if (properties[j].PropertyName == info.PropertyName)
                        {
                            previousPropertyIndex = j;
                            previousPropertyWithName = properties[j];
                            break;
                        }
                    }

                    if (previousPropertyWithName != null)
                    {
                        bool createAlias = true;

                        if (member.SignaturesAreEqual(previousPropertyWithName.GetMemberInfo()))
                        {
                            createAlias = false;
                            properties.RemoveAt(previousPropertyIndex);
                        }

                        //if (previousPropertyWithName.PropertyType == PropertyType.Method && info.PropertyType == PropertyType.Method)
                        //{
                        //    var oldMethod = (MethodInfo)previousPropertyWithName.GetMemberInfo();
                        //    var newMethod = (MethodInfo)member;

                        //    if (MembersAreSame(oldMethod, newMethod))
                        //    {
                        //        createAlias = false;
                        //        properties.RemoveAt(previousPropertyIndex);
                        //    }

                        //    //if (oldMethod.GetBaseDefinition() == newMethod.GetBaseDefinition())
                        //    //{
                        //    //    // We have encountered an override of a method that is already a property
                        //    //    // This is a special case; we remove the base method property, and keep
                        //    //    // only the override method property.

                        //    //    createAlias = false;
                        //    //    properties.RemoveAt(previousPropertyIndex);
                        //    //}
                        //}

                        if (createAlias)
                        {
                            var alias = GetPrivateMemberAlias(previousPropertyWithName.GetMemberInfo(), previousPropertyWithName.TypeOfOwner.GetNiceName(), " -> ");

                            var aliasName = alias.Name;

                            InspectorPropertyInfo aliasedProperty;

                            if (InspectorPropertyInfoUtility.TryCreate(parentProperty, alias, includeSpeciallySerializedMembers, out aliasedProperty))
                            {
                                //Debug.LogWarning("The inspector property '" + hidden + "' hides inherited property '" + inherited + "'. Alias property '" + aliasName + "' created for inherited property '" + inherited + "'.");
                                properties[previousPropertyIndex] = aliasedProperty;
                            }
                            else
                            {
                                var hidden = info.TypeOfOwner.GetNiceName() + "." + info.GetMemberInfo().Name;
                                var inherited = previousPropertyWithName.TypeOfOwner.GetNiceName() + "." + previousPropertyWithName.PropertyName;

                                //Debug.LogWarning("The inspector property '" + hidden + "' hides inherited property '" + inherited + "'. Failed to create alias property '" + aliasName + "' for inherited property '" + inherited + "'; removing inherited property instead.");
                                properties.RemoveAt(previousPropertyIndex);
                            }
                        }
                    }

                    properties.Add(info);
                }
            }
        }

        private static SerializationBackend GetSerializationBackendOfProperty(InspectorProperty property)
        {
            // Find the nearest parent property that has a value, if that exists
            if (property.ValueEntry == null) property = property.ParentValueProperty ?? property;

            return property.Info.SerializationBackend;
        }

        public static SerializationBackend GetSerializationBackend(InspectorProperty parentProperty, MemberInfo member)
        {
            return GetSerializationBackend(parentProperty, member, GetSerializationBackendOfProperty(parentProperty));
        }

        private static SerializationBackend GetSerializationBackend(InspectorProperty parentProperty, MemberInfo member, SerializationBackend parentBackend)
        {
            if (!(member is FieldInfo || member is PropertyInfo)) return SerializationBackend.None;

            InspectorProperty serializationRoot;

            if (parentProperty.ValueEntry == null)
                parentProperty = parentProperty.ParentValueProperty ?? parentProperty;

            // Determine the serialization root
            {
                if (parentProperty.ValueEntry != null && typeof(UnityEngine.Object).IsAssignableFrom(parentProperty.ValueEntry.TypeOfValue))
                {
                    serializationRoot = parentProperty;
                }
                else
                {
                    serializationRoot = parentProperty.SerializationRoot;
                }
            }
            
            // Early out for properties whose serialization root has no value (statically inspected property trees, for example).
            if (serializationRoot.ValueEntry == null) return SerializationBackend.None;

            if (parentBackend == SerializationBackend.None && serializationRoot != parentProperty)
            {
                // Early out for all properties that are children of a non-serialized property that is not a serialization root.
                return SerializationBackend.None;
            }

            
            ISerializationPolicy policy = SerializationPolicies.Unity;
            bool includeSpeciallySerializedMembers = TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(serializationRoot.ValueEntry.TypeOfValue);
            IOverridesSerializationPolicy policyOverride = serializationRoot.ValueEntry.WeakValues[0] as IOverridesSerializationPolicy;

            if (includeSpeciallySerializedMembers && policyOverride != null)
            {
                policy = policyOverride.SerializationPolicy ?? SerializationPolicies.Unity;
            }

            if (serializationRoot != parentProperty)
            {
                // This is not the immediate child of a serialization root, meaning the serialization is dependent on the serialization of the parent value property
                if (parentBackend == SerializationBackend.Odin)
                {
                    return UnitySerializationUtility.OdinWillSerialize(member, true, policy) ? SerializationBackend.Odin : SerializationBackend.None;
                }

                if (parentBackend.IsUnity)
                {
                    if (SerializationBackend.UnityPolymorphic.CanSerializeMember(member))
                        return SerializationBackend.UnityPolymorphic;

                    return SerializationBackend.Unity.CanSerializeMember(member) ? SerializationBackend.Unity : SerializationBackend.None;
                }

                if (parentBackend.CanSerializeMember(member))
                {
                    return parentBackend;
                }

                return SerializationBackend.None;
            }
            else
            {
                // This is the immediate child of a serialization root
                if (includeSpeciallySerializedMembers)
                {
                    // The root has Odin serialization
                    bool serializeUnityFields = false;

                    if (policyOverride != null) serializeUnityFields = policyOverride.OdinSerializesUnityFields;

                    if (UnitySerializationUtility.OdinWillSerialize(member, serializeUnityFields, policy))
                        return SerializationBackend.Odin;
                }

                if (SerializationBackend.UnityPolymorphic.CanSerializeMember(member))
                    return SerializationBackend.UnityPolymorphic;

                if (SerializationBackend.Unity.CanSerializeMember(member))
                    return SerializationBackend.Unity;
            }

            return SerializationBackend.None;
        }

        public static void ProcessAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            var processors = parentProperty.Tree.AttributeProcessorLocator.GetChildProcessors(parentProperty, member);

            for (int i = 0; i < processors.Count; i++)
            {
                try
                {
                    processors[i].ProcessChildMemberAttributes(parentProperty, member, attributes);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static bool TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(Type type)
        {
            bool result;

            if (!TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cache.TryGetValue(type, out result))
            {
                result = type.IsDefined(typeof(ShowOdinSerializedPropertiesInInspectorAttribute), true);
                TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cache.Add(type, result);
            }

            return result;
        }

        private struct GroupAttributeInfo
        {
            public InspectorPropertyInfo InspectorPropertyInfo;
            public PropertyGroupAttribute Attribute;
            public bool Exclude;
        }

        private class GroupData
        {
            public string Name;
            public string ID;
            public GroupData Parent;
            public PropertyGroupAttribute ConsolidatedAttribute;
            public List<GroupAttributeInfo> Attributes = new List<GroupAttributeInfo>();
            public readonly List<GroupData> ChildGroups = new List<GroupData>();
        }
    }
}
#endif