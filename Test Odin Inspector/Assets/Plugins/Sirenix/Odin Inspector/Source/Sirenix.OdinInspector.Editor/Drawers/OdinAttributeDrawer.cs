#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    // If you make changes to the documentation, make sure to do it in both OdinAttributeDrawer<TAttribute> and OdinAttributeDrawer<TAttribute, TValue>

    /// <summary>
    /// <para>
    /// Base class for attribute drawers. Use this class to create your own custom attribute drawers that will work for all types.
    /// Alternatively you can derive from <see cref="OdinAttributeDrawer{TAttribute, TValue}"/> if you want to only support specific types.
    /// </para>
    ///
    /// <para>
    /// Odin supports the use of GUILayout and takes care of undo for you. It also takes care of multi-selection
    /// in many simple cases. Check the manual for more information on handling multi-selection.
    /// </para>
    ///
    /// <para>
    /// Also note that Odin does not require that your custom attribute inherits from Unity's PropertyAttribute.
    /// </para>
    /// </summary>
    /// 
    /// <typeparam name="TAttribute">The attribute that this drawer should be applied to.</typeparam>
    ///
    /// <remarks>
    /// Checkout the manual for more information.
    /// </remarks>
    ///
    /// <example>
    /// <para>Example using the <see cref="OdinAttributeDrawer{TAttribute, TValue}"/>.</para>
    /// <code>
    /// [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    /// public class CustomRangeAttribute : System.Attribute
    /// {
    ///     public float Min;
    ///     public float Max;
    ///
    ///     public CustomRangeAttribute(float min, float max)
    ///     {
    ///         this.Min = min;
    ///         this.Max = max;
    ///     }
    /// }
    ///
    /// // Remember to wrap your custom attribute drawer within a #if UNITY_EDITOR condition, or locate the file inside an Editor folder.
    ///
    /// public sealed class CustomRangeAttributeDrawer : OdinAttributeDrawer&lt;CustomRangeAttribute, float&gt;
    /// {
    ///     protected override void DrawPropertyLayout(GUIContent label)
    ///     {
    ///         this.ValueEntry.SmartValue = EditorGUILayout.Slider(label, this.ValueEntry.SmartValue, this.Attribute.Min, this.Attribute.Max);
    ///     }
    /// }
    ///
    /// // Usage:
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [CustomRangeAttribute(0, 1)]
    ///     public float MyFloat;
    /// }
    /// </code>
    /// </example>
    ///
    /// <example>
    /// <para>Example using the <see cref="OdinAttributeDrawer{TAttribute}"/>.</para>
    /// <code>
    /// [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    /// public class GUITintColorAttribute : System.Attribute
    /// {
    ///     public Color Color;
    ///
    ///     public GUITintColorAttribute(float r, float g, float b, float a = 1)
    ///     {
    ///         this.Color = new Color(r, g, b, a);
    ///     }
    /// }
    ///
    /// // Remember to wrap your custom attribute drawer within a #if UNITY_EDITOR condition, or locate the file inside an Editor folder.
    ///
    /// public sealed class GUITintColorAttributeDrawer : OdinAttributeDrawer&lt;GUITintColorAttribute&gt;
    /// {
    ///     protected override void DrawPropertyLayout(GUIContent label)
    ///     {
    ///        Color prevColor = GUI.color;
    ///        GUI.color *= this.Attribute.Color;
    ///        this.CallNextDrawer(label);
    ///        GUI.color = prevColor;
    ///     }
    /// }
    ///
    /// // Usage:
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [GUITintColor(0, 1, 0)]
    ///     public float MyFloat;
    /// }
    /// </code>
    /// </example>
    ///
    /// <example>
    /// <para>
    /// Odin uses multiple drawers to draw any given property, and the order in which these drawers are
    /// called are defined using the <see cref="DrawerPriorityAttribute"/>.
    /// Your custom drawer injects itself into this chain of drawers based on its <see cref="DrawerPriorityAttribute"/>.
    /// If no <see cref="DrawerPriorityAttribute"/> is defined, a priority is generated automatically based on the type of the drawer.
    /// Each drawer can ether choose to draw the property or not, or pass on the responsibility to the
    /// next drawer by calling CallNextDrawer(), as the f attribute does in the example above.
    /// </para>
    ///
    /// <para>
    /// This means that there is no guarantee that your drawer will be called, sins other drawers
    /// could have a higher priority than yours and choose not to call CallNextDrawer().
    /// </para>
    ///
    /// <para>
    /// Note that Odin's <see cref="DefaultDrawerChainResolver" /> has full support for generic class constraints,
    /// and if that is not enough, you can also add additional type constraints by overriding CanDrawTypeFilter
    /// </para>
    ///
    /// <para>
    /// Also note that all custom property drawers needs to handle cases where the label provided by the DrawPropertyLayout is null,
    /// otherwise exceptions will be thrown when in cases where the label is hidden. For instance when [HideLabel] is used, or the property is drawn within a list where labels are also not shown.
    /// </para>
    ///
    /// <code>
    /// [DrawerPriority(DrawerPriorityLevel.AttributePriority)]
    /// public sealed class MyCustomAttributeDrawer&lt;T&gt; : OdinAttributeDrawer&lt;MyCustomAttribute, T&gt; where T : class
    /// {
    ///     public override bool CanDrawTypeFilter(Type type)
    ///     {
    ///         return type != typeof(string);
    ///     }
    ///
    ///     protected override void DrawPropertyLayout(GUIContent label)
    ///     {
    ///         // Draw property here.
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinAttributeDrawer{TAttribute, TValue}"/>
    /// <seealso cref="OdinValueDrawer{T}"/>
    /// <seealso cref="OdinGroupDrawer{TGroupAttribute}"/>
    /// <seealso cref="OdinDrawer"/>
    /// <seealso cref="InspectorProperty"/>
    /// <seealso cref="OdinDrawerAttribute"/>
    /// <seealso cref="DrawerPriorityAttribute"/>
    /// <seealso cref="DrawerLocator"/>
    /// <seealso cref="InspectorUtilities"/>
    /// <seealso cref="PropertyTree"/>
    /// <seealso cref="Sirenix.Utilities.Editor.GUIHelper"/>
    /// <seealso cref="Sirenix.Utilities.Editor.SirenixEditorGUI"/>
    public abstract class OdinAttributeDrawer<TAttribute> : OdinDrawer where TAttribute : Attribute
    {
        private TAttribute attribute;

        /// <summary>
        /// Gets the attribute that the OdinAttributeDrawer draws for.
        /// </summary>
        public TAttribute Attribute
        {
            get
            {
                if (this.attribute == null)
                {
                    var chain = this.Property.GetActiveDrawerChain();

                    int count = 0;

                    for (int i = 0; i < chain.BakedDrawerArray.Length; i++)
                    {
                        var drawer = chain.BakedDrawerArray[i];

                        if (drawer.GetType() == this.GetType())
                        {
                            if (object.ReferenceEquals(this, drawer)) break;
                            else count++;
                        }
                    }

                    int savedCount = count;
                    var type = typeof(TAttribute);

                    for (int i = 0; i < this.Property.Attributes.Count; i++)
                    {
                        var attr = this.Property.Attributes[i];

                        if (attr.GetType() != type) continue;

                        if (count == 0)
                        {
                            this.attribute = (TAttribute)attr;
                            break;
                        }
                        else count--;
                    }

                    if (this.attribute == null)
                    {
                        Debug.LogError("Could not find attribute '" + typeof(TAttribute).GetNiceName() + "' number " + savedCount + " for the drawer '" + this.GetType().GetNiceName() + "' number " + savedCount + "; not enough attributes of the required type on the property - why are there more drawers for the attribute than there are attributes?");
                        this.attribute = this.Property.GetAttribute<TAttribute>();
                    }
                }

                return this.attribute;
            }
        }

        /// <summary>
        /// Tells whether or not multiple attributes are allowed.
        /// </summary>
        protected static bool AllowsMultipleAttributes;

        static OdinAttributeDrawer()
        {
            AttributeUsageAttribute attr = typeof(TAttribute).GetAttribute<AttributeUsageAttribute>(true);

            if (attr != null)
            {
                OdinAttributeDrawer<TAttribute>.AllowsMultipleAttributes = attr.AllowMultiple;
            }
        }
        
        /// <summary>
        /// Draws the property with the given label.
        /// Override this to implement your custom OdinAttributeDrawer.
        /// </summary>
        /// <param name="label">Optional label for the property.</param>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.LabelField(label, "The DrawPropertyLayout method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "', or you are calling base.DrawPropertyLayout(label).");
        }

        /// <summary>
        /// Tests if the drawer can draw for the specified property.
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// <returns><c>true</c> if the drawer can drawn the property. Otherwise <c>false</c>.</returns>
        public sealed override bool CanDrawProperty(InspectorProperty property)
        {
            if (property.ValueEntry != null && !this.CanDrawTypeFilter(property.ValueEntry.TypeOfValue)) return false;
            return property.GetAttribute<TAttribute>() != null && this.CanDrawAttributeProperty(property);
        }

        /// <summary>
        /// Tests if the attribute drawer can draw for the specified property.
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// <returns><c>true</c> if the drawer can drawn the property. Otherwise <c>false</c>.</returns>
        protected virtual bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return true;
        }
    }

    /// <summary>
    /// <para>
    /// Base class for all type specific attribute drawers. For non-type specific attribute drawers see <see cref="OdinAttributeDrawer{TAttribute, TValue}"/>.
    /// </para>
    ///
    /// <para>
    /// Odin supports the use of GUILayout and takes care of undo for you. It also takes care of multi-selection
    /// in many simple cases. Checkout the manual for more information on handling multi-selection.
    /// </para>
    ///
    /// <para>
    /// Also note that Odin does not require that your custom attribute inherits from Unity's PropertyAttribute.
    /// </para>
    /// </summary>
    /// 
    /// <typeparam name="TAttribute">The attribute that this drawer should be applied to.</typeparam>
    /// <typeparam name="TValue">The type of the value the drawer should be drawing. Note that Odin's <see cref="DefaultDrawerChainResolver"/> has full support for generic constraints.</typeparam>
    ///
    /// <remarks>
    /// Checkout the manual for more information.
    /// </remarks>
    ///
    /// <example>
    /// <para>Example using the <see cref="OdinAttributeDrawer{TAttribute, TValue}"/>.</para>
    /// <code>
    /// [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    /// public class CustomRangeAttribute : System.Attribute
    /// {
    ///     public float Min;
    ///     public float Max;
    ///
    ///     public CustomRangeAttribute(float min, float max)
    ///     {
    ///         this.Min = min;
    ///         this.Max = max;
    ///     }
    /// }
    ///
    /// // Remember to wrap your custom attribute drawer within a #if UNITY_EDITOR condition, or locate the file inside an Editor folder.
    ///
    /// public sealed class CustomRangeAttributeDrawer : OdinAttributeDrawer&lt;CustomRangeAttribute, float&gt;
    /// {
    ///     protected override void DrawPropertyLayout(GUIContent label)
    ///     {
    ///         this.ValueEntry.SmartValue = EditorGUILayout.Slider(label, this.ValueEntry.SmartValue, this.Attribute.Min, this.Attribute.Max);
    ///     }
    /// }
    ///
    /// // Usage:
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [CustomRangeAttribute(0, 1)]
    ///     public float MyFloat;
    /// }
    /// </code>
    /// </example>
    ///
    /// <example>
    /// <para>Example using the <see cref="OdinAttributeDrawer{TAttribute}"/>.</para>
    /// <code>
    /// [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    /// public class GUITintColorAttribute : System.Attribute
    /// {
    ///     public Color Color;
    ///
    ///     public GUITintColorAttribute(float r, float g, float b, float a = 1)
    ///     {
    ///         this.Color = new Color(r, g, b, a);
    ///     }
    /// }
    ///
    /// // Remember to wrap your custom attribute drawer within a #if UNITY_EDITOR condition, or locate the file inside an Editor folder.
    ///
    /// public sealed class GUITintColorAttributeDrawer : OdinAttributeDrawer&lt;GUITintColorAttribute&gt;
    /// {
    ///     protected override void DrawPropertyLayout(GUIContent label)
    ///     {
    ///        Color prevColor = GUI.color;
    ///        GUI.color *= this.Attribute.Color;
    ///        this.CallNextDrawer(label);
    ///        GUI.color = prevColor;
    ///     }
    /// }
    ///
    /// // Usage:
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [GUITintColor(0, 1, 0)]
    ///     public float MyFloat;
    /// }
    /// </code>
    /// </example>
    ///
    /// <example>
    /// <para>
    /// Odin uses multiple drawers to draw any given property, and the order in which these drawers are
    /// called is defined using the <see cref="DrawerPriorityAttribute"/>.
    /// Your custom drawer injects itself into this chain of drawers based on its <see cref="DrawerPriorityAttribute"/>.
    /// If no <see cref="DrawerPriorityAttribute"/> is defined, a priority is generated automatically based on the type of the drawer.
    /// Each drawer can ether choose to draw the property or not, or pass on the responsibility to the
    /// next drawer by calling CallNextDrawer(), as the GUITintColor attribute does in the example above.
    /// </para>
    ///
    /// <para>
    /// This means that there is no guarantee that your drawer will be called, since other drawers
    /// could have a higher priority than yours and choose not to call CallNextDrawer().
    /// </para>
    ///
    /// <para>
    /// Note that Odin's <see cref="DefaultDrawerChainResolver" /> has full support for generic class constraints,
    /// and if that is not enough, you can also add additional type constraints by overriding CanDrawTypeFilter
    /// </para>
    ///
    /// <para>
    /// Also note that all custom property drawers needs to handle cases where the label provided by the DrawPropertyLayout is null,
    /// otherwise exceptions will be thrown when in cases where the label is hidden. For instance when [HideLabel] is used, or the property is drawn within a list where labels are also not shown.
    /// </para>
    ///
    /// <code>
    /// [DrawerPriority(DrawerPriorityLevel.AttributePriority)]
    /// public class MyCustomAttributeDrawer&lt;T&gt; : OdinAttributeDrawer&lt;MyCustomAttribute, T&gt; where T : class
    /// {
    ///     public override bool CanDrawTypeFilter(Type type)
    ///     {
    ///         return type != typeof(string);
    ///     }
    ///
    ///     protected override void DrawPropertyLayout(GUIContent label)
    ///     {
    ///         // Draw property here.
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinAttributeDrawer{TAttribute}"/>
    /// <seealso cref="OdinValueDrawer{T}"/>
    /// <seealso cref="OdinGroupDrawer{TGroupAttribute}"/>
    /// <seealso cref="OdinDrawer"/>
    /// <seealso cref="InspectorProperty"/>
    /// <seealso cref="OdinDrawerAttribute"/>
    /// <seealso cref="DrawerPriorityAttribute"/>
    /// <seealso cref="DrawerLocator"/>
    /// <seealso cref="InspectorUtilities"/>
    /// <seealso cref="PropertyTree"/>
    /// <seealso cref="Sirenix.Utilities.Editor.GUIHelper"/>
    /// <seealso cref="Sirenix.Utilities.Editor.SirenixEditorGUI"/>
    public abstract class OdinAttributeDrawer<TAttribute, TValue> : OdinAttributeDrawer<TAttribute> where TAttribute : Attribute
    {
        private IPropertyValueEntry<TValue> valueEntry;

        /// <summary>
        /// Gets the strongly typed ValueEntry of the OdinAttributeDrawer's property.
        /// </summary>
        public IPropertyValueEntry<TValue> ValueEntry
        {
            get
            {
                if (this.valueEntry == null)
                {
                    this.valueEntry = this.Property.ValueEntry as IPropertyValueEntry<TValue>;

                    if (this.valueEntry == null)
                    {
                        this.Property.Update(true);
                        this.valueEntry = this.Property.ValueEntry as IPropertyValueEntry<TValue>;
                    }
                }

                return this.valueEntry;
            }
        }

        /// <summary>
        /// Draws the property with the given label.
        /// Override this to implement your custom OdinAttributeDrawer.
        /// </summary>
        /// <param name="label">Optional label for the property.</param>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.LabelField(label, "The DrawPropertyLayout method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "'.");
        }

        /// <summary>
        /// Tests if the drawer can draw for the specified property.
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// <returns><c>true</c> if the drawer can drawn the property. Otherwise <c>false</c>.</returns>
        protected sealed override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return property.ValueEntry != null
                && property.ValueEntry.TypeOfValue == typeof(TValue)
                && this.CanDrawAttributeValueProperty(property);
        }

        /// <summary>
        /// Tests if the attribute drawer can draw for the specified property.
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// <returns><c>true</c> if the drawer can drawn the property. Otherwise <c>false</c>.</returns>
        protected virtual bool CanDrawAttributeValueProperty(InspectorProperty property)
        {
            return true;
        }
    }
}
#endif