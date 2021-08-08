#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinGroupDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// <para>
    /// Base class for all group drawers. Use this class to create your own custom group drawers. OdinGroupDrawer are used to group multiple properties together using an attribute.
    /// </para>
    ///
    /// <para>
    /// Note that all box group attributes needs to inherit from the <see cref="PropertyGroupAttribute"/>
    /// </para>
    ///
    /// <para>
    /// Remember to provide your custom drawer with an <see cref="Sirenix.OdinInspector.Editor.OdinDrawerAttribute"/>
    /// in order for it to be located by the <see cref="DrawerLocator"/>.
    /// </para>
    ///
    /// </summary>
    ///
    /// <remarks>
    /// Checkout the manual for more information.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    /// public class MyBoxGroupAttribute : PropertyGroupAttribute
    /// {
    ///     public MyBoxGroupAttribute(string group, float order = 0) : base(group, order)
    ///     {
    ///     }
    /// }
    ///
    /// // Remember to wrap your custom group drawer within a #if UNITY_EDITOR condition, or locate the file inside an Editor folder.
    ///
    /// public class BoxGroupAttributeDrawer : OdinGroupDrawer&lt;MyBoxGroupAttribute&gt;
    /// {
    ///     protected override void DrawPropertyGroupLayout(InspectorProperty property, MyBoxGroupAttribute attribute, GUIContent label)
    ///     {
    ///         GUILayout.BeginVertical("box");
    ///         for (int i = 0; i &lt; property.Children.Count; i++)
    ///         {
    ///             InspectorUtilities.DrawProperty(property.Children[i]);
    ///         }
    ///         GUILayout.EndVertical();
    ///     }
    /// }
    ///
    /// // Usage:
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [MyBoxGroup("MyGroup")]
    ///     public int A;
    ///
    ///     [MyBoxGroup("MyGroup")]
    ///     public int B;
    ///
    ///     [MyBoxGroup("MyGroup")]
    ///     public int C;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinAttributeDrawer{TAttribute}"/>
    /// <seealso cref="OdinAttributeDrawer{TAttribute, TValue}"/>
    /// <seealso cref="OdinValueDrawer{T}"/>
    /// <seealso cref="OdinDrawer"/>
    /// <seealso cref="InspectorProperty"/>
    /// <seealso cref="OdinDrawerAttribute"/>
    /// <seealso cref="DrawerPriorityAttribute"/>
    /// <seealso cref="DrawerLocator"/>
    /// <seealso cref="InspectorUtilities"/>
    /// <seealso cref="PropertyTree"/>
    /// <seealso cref="Sirenix.Utilities.Editor.GUIHelper"/>
    /// <seealso cref="Sirenix.Utilities.Editor.SirenixEditorGUI"/>
    public abstract class OdinGroupDrawer<TGroupAttribute> : OdinDrawer where TGroupAttribute : PropertyGroupAttribute
    {
        private TGroupAttribute attribute;

        public TGroupAttribute Attribute
        {
            get
            {
                if (this.attribute == null)
                {
                    this.attribute = this.Property.GetAttribute<TGroupAttribute>();

                    if (this.attribute == null)
                    {
                        this.attribute = this.Property.Info.GetAttribute<TGroupAttribute>();
                    }

                    if (this.attribute == null)
                    {
                        Debug.LogError("Property group " + this.Property.Name + " does not have an attribute of the required type " + typeof(TGroupAttribute).GetNiceName() + ".");
                    }
                }

                return this.attribute;
            }
        }

        /// <summary>
        /// Draws the property with GUILayout support.
        /// </summary>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.LabelField(label, "The DrawPropertyLayout method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "', or you are calling base.DrawPropertyLayout(label).");
        }

        public sealed override bool CanDrawProperty(InspectorProperty property)
        {
            return property.Info.PropertyType == PropertyType.Group && this.CanDrawGroup(property);
        }

        protected virtual bool CanDrawGroup(InspectorProperty property)
        {
            return true;
        }
    }
}
#endif