#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// <para>
    /// Base class for all Odin drawers. In order to create your own custom drawers you need to derive from one of the following drawers:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="OdinAttributeDrawer{TAttribute}"/></item>
    /// <item><see cref="OdinAttributeDrawer{TAttribute, TValue}"/></item>
    /// <item><see cref="OdinValueDrawer{T}"/></item>
    /// <item><see cref="OdinGroupDrawer{TGroupAttribute}"/></item>
    /// </list>
    /// <para>Remember to provide your custom drawer with an <see cref="Sirenix.OdinInspector.Editor.OdinDrawerAttribute"/> in order for it to be located by the <see cref="DrawerLocator"/>.</para>
    /// <para>Drawers require a <see cref="PropertyTree"/> context, and are instantiated automatically by the <see cref="DrawerLocator"/>.</para>
    /// <para>Odin supports the use of GUILayout and takes care of undo for you. It also takes care of multi-selection in many simple cases. Checkout the manual for more information.</para>
    /// </summary>
    /// <seealso cref="OdinAttributeDrawer{TAttribute}"/>
    /// <seealso cref="OdinAttributeDrawer{TAttribute, TValue}"/>
    /// <seealso cref="OdinValueDrawer{T}"/>
    /// <seealso cref="OdinGroupDrawer{TGroupAttribute}"/>
    /// <seealso cref="InspectorProperty"/>
    /// <seealso cref="Sirenix.OdinInspector.Editor.OdinDrawerAttribute"/>
    /// <seealso cref="DrawerPriorityAttribute"/>
    /// <seealso cref="DrawerLocator"/>
    /// <seealso cref="InspectorUtilities"/>
    /// <seealso cref="PropertyTree"/>
    /// <seealso cref="Sirenix.Utilities.Editor.GUIHelper"/>
    /// <seealso cref="Sirenix.Utilities.Editor.SirenixEditorGUI"/>
    public abstract class OdinDrawer
    {
        private bool initialized;
        private InspectorProperty property;

        /// <summary>
        /// If <c>true</c> then this drawer will be skipped in the draw chain. Otherwise the drawer will be called as normal in the draw chain.
        /// </summary>
        public bool SkipWhenDrawing { get; set; }

        /// <summary>
        /// Gets a value indicating if the drawer has been initialized yet.
        /// </summary>
        public bool Initialized { get { return this.initialized; } }

        /// <summary>
        /// Gets the property this drawer draws for.
        /// </summary>
        public InspectorProperty Property { get { return this.property; } }

        /// <summary>
        /// <para>Override this method in order to define custom type constraints to specify whether or not a type should be drawn by the drawer.</para>
        /// <para>Note that Odin's <see cref="DrawerLocator" /> has full support for generic class constraints, so most often you can get away with not overriding CanDrawTypeFilter.</para>
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// Returns true by default, unless overridden.
        /// </returns>
        public virtual bool CanDrawTypeFilter(Type type)
        {
            return true;
        }

        /// <summary>
        /// Initializes the drawer instance.
        /// </summary>
        /// <param name="property"></param>
        public void Initialize(InspectorProperty property)
        {
            if (this.initialized) return;

            this.property = property;

            try
            {
                this.Initialize();
            }
            finally
            {
                this.initialized = true;
            }
        }

        /// <summary>
        /// Initializes the drawer instance. Override this to implement your own initialization logic.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Draws the property with a custom label.
        /// </summary>
        /// <param name="label">The label. Null is allow if you wish no label should be drawn.</param>
        public void DrawProperty(GUIContent label)
        {
            if (!this.initialized) throw new InvalidOperationException("Cannot call DrawProperty on a drawer before it has been initialized!");

            this.DrawPropertyLayout(label);
        }
        
        /// <summary>
        /// Draws the property with GUILayout support.
        /// </summary>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        protected virtual void DrawPropertyLayout(GUIContent label)
        {
            if (label != null)
            {
                EditorGUILayout.LabelField(label, new GUIContent("The DrawPropertyLayout method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "', or you are calling base.DrawPropertyLayout(label), which you shouldn't."));
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent("The DrawPropertyLayout method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "', or you are calling base.DrawPropertyLayout(label), which you shouldn't."));
            }
        }
        
        /// <summary>
        /// Calls the next drawer in the draw chain.
        /// </summary>
        /// <param name="label">The label to pass on to the next drawer.</param>
        protected bool CallNextDrawer(GUIContent label)
        {
            //var nextDrawer = DrawerLocator.GetNextDrawer(this, property);

            OdinDrawer nextDrawer = null;

            var chain = this.property.GetActiveDrawerChain();

            if (chain.MoveNext())
            {
                nextDrawer = chain.Current;
            }

            if (nextDrawer != null)
            {
#if ODIN_TRIAL
                bool former = true;
                if (TrialUtilities.IsReallyExpired)
                {
                    former = GUI.enabled;
                    GUI.enabled = false;
                }
#endif
                nextDrawer.DrawPropertyLayout(label);

#if ODIN_TRIAL
                if (TrialUtilities.IsReallyExpired)
                {
                    GUI.enabled = former;
                }
#endif
                return true;
            }
            else if (property.ValueEntry != null)
            {
                var rect = EditorGUILayout.GetControlRect();
                if (label == null)
                {
                    GUI.Label(rect, this.Property.NiceName);
                }
                else
                {
                    GUI.Label(rect, label);
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    if (label != null)
                    {
                        EditorGUILayout.PrefixLabel(label);
                    }
                    SirenixEditorGUI.WarningMessageBox("There is no drawer defined for property " + property.NiceName + " of type " + property.Info.PropertyType + ".");
                }
                GUILayout.EndHorizontal();
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating if the drawer can draw for the specified property.
        /// Override this to implement a custom property filter for your drawer.
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// <returns><c>true</c> if the drawer can draw for the property. Otherwise <c>false</c>.</returns>
        public virtual bool CanDrawProperty(InspectorProperty property)
        {
            return true;
        }
    }
}
#endif