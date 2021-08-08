#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinSerializeAttributeWarningDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// <para>
    /// When first learning to use the Odin Inspector, it is common for people to misunderstand the OdinSerialize attribute,
    /// and use it in places where it does not achive the deceired goal.
    /// </para>
    /// <para>
    /// This drawer will display a warning message if the OdinSerialize attribute is potentially used in such cases.
    /// </para>
    /// </summary>
    /// <seealso cref="Sirenix.OdinInspector.Editor.OdinAttributeDrawer{Sirenix.Serialization.OdinSerializeAttribute}" />
    [DrawerPriority(1000, 0, 0)]
    public sealed class OdinSerializeAttributeWarningDrawer : OdinAttributeDrawer<OdinSerializeAttribute>
    {
        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            if (GlobalSerializationConfig.Instance.HideOdinSerializeAttributeWarningMessages)
            {
                return false;
            }

            return property.Parent != null && property.Parent.IsTreeRoot;
        }

        private string message;

        protected override void Initialize()
        {
            var property = this.Property;

            if (property.ValueEntry.SerializationBackend == SerializationBackend.None)
            {
                this.message = "The following property is marked with the [OdinSerialize] attribute, " +
                          "but the property is not part of any object that uses the Odin Serialization Protocol. \n\n" +
                          "Are you perhaps forgetting to inherit from one of our serialized base classes such as SerializedMonoBehaviour or SerializedScriptableObject? \n\n";

                var fieldInfo = property.Info.GetMemberInfo() as System.Reflection.FieldInfo;
                if (fieldInfo != null && fieldInfo.IsPublic && property.Info.GetAttribute<System.NonSerializedAttribute>() == null)
                {
                    this.message += "Odin will also serialize public fields by default, so are you sure you need to mark the field with the [OdinSerialize] attribute?\n\n";
                }
            }                                                                                 // We need a way to find out if Unity will also serialize it.
            else if (property.ValueEntry.SerializationBackend == SerializationBackend.Odin && UnitySerializationUtility.GuessIfUnityWillSerialize(property.Info.GetMemberInfo()))
            {
                this.message = "The following property is marked with the [OdinSerialize] attribute, but Unity is also serializing it. " +
                          "You can either remove the [OdinSerialize] attribute and let Unity serialize it, or you can use the [NonSerialized] " +
                          "attribute together with the [OdinSerialize] attribute if you want Odin to serialize it instead of Unity.\n\n";

                bool isCustomSerializableType =
                    property.Info.TypeOfOwner.GetAttribute<System.SerializableAttribute>() != null &&
                    (property.Info.TypeOfOwner.Assembly.GetAssemblyTypeFlag() & AssemblyTypeFlags.CustomTypes) != 0;

                if (isCustomSerializableType)
                {
                    this.message += "Odin's default serialization protocol does not require a type to be marked with the [Serializable] attribute in order for it to be serialized, which Unity does. " +
                               "Therefore you could remove the System.Serializable attribute from " + property.Info.TypeOfOwner.GetNiceFullName() + " if you want Unity never to serialize the type.\n\n";
                }
            }

            if (this.message != null)
            {
                this.message += "Check out our online manual for more information.\n\n";
                this.message += "This message can be disabled in the 'Tools > Odin Inspector > Preferences > Serialization' window, but it is recommended that you don't.";
            }
        }

        /// <summary>
        /// Draws The Property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            //if (GlobalSerializationConfig.Instance.HideOdinSerializeAttributeWarningMessages || this.Property.Parent != null)
            //{
            //    this.CallNextDrawer(label);
            //    return;
            //}

            if (this.message != null)
            {
                SirenixEditorGUI.WarningMessageBox(this.message);
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif