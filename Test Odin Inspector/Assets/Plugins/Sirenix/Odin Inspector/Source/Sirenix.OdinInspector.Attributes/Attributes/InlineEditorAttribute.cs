//-----------------------------------------------------------------------
// <copyright file="InlineEditorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>InlineAttribute is used on any property or field with a type that inherits from UnityEngine.Object. This includes components and assets etc.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// public class InlineEditorExamples : MonoBehaviour
    /// {
    ///     [DisableInInlineEditors]
    ///     public Vector3 DisabledInInlineEditors;
    ///    
    ///     [HideInInlineEditors]
    ///     public Vector3 HiddenInInlineEditors;
    ///    
    ///     [InlineEditor]
    ///     public Transform InlineComponent;
    /// 
    ///     [InlineEditor(InlineEditorModes.FullEditor)]
    ///     public Material FullInlineEditor;
    /// 
    ///     [InlineEditor(InlineEditorModes.GUIAndHeader)]
    ///     public Material InlineMaterial;
    /// 
    ///     [InlineEditor(InlineEditorModes.SmallPreview)]
    ///     public Material[] InlineMaterialList;
    /// 
    ///     [InlineEditor(InlineEditorModes.LargePreview)]
    ///     public GameObject InlineObjectPreview;
    /// 
    ///     [InlineEditor(InlineEditorModes.LargePreview)]
    ///     public Mesh InlineMeshPreview;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="DisableInInlineEditorsAttribute"/>
    /// <seealso cref="HideInInlineEditorsAttribute"/>
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class InlineEditorAttribute : Attribute
    {
        private bool expanded;

        /// <summary>
        /// If true, the inline editor will start expanded.
        /// </summary>
        public bool Expanded
        {
            get { return this.expanded; }
            set
            {
                this.expanded = value;
                this.ExpandedHasValue = true;
            }
        }

        /// <summary>
        /// Draw the header editor header inline.
        /// </summary>
        public bool DrawHeader;

        /// <summary>
        /// Draw editor GUI inline.
        /// </summary>
        public bool DrawGUI;

        /// <summary>
        /// Draw editor preview inline.
        /// </summary>
        public bool DrawPreview;

        /// <summary>
        /// Maximum height of the inline editor. If the inline editor exceeds the specified height, a scrollbar will appear.
        /// Values less or equals to zero will let the InlineEditor expand to its full size. 
        /// </summary>
        public float MaxHeight;

        /// <summary>
        /// The size of the editor preview if drawn together with GUI.
        /// </summary>
        public float PreviewWidth = 100;

        /// <summary>
        /// The size of the editor preview if drawn alone.
        /// </summary>
        public float PreviewHeight = 35;

        /// <summary>
        /// If false, this will prevent the InlineEditor attribute from incrementing the InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth. 
        /// This is helpful in cases where you want to draw the entire editor, and disregard attributes 
        /// such as [<see cref="HideInInlineEditorsAttribute"/>] and [<see cref="DisableInInlineEditorsAttribute"/>].
        /// </summary>
        public bool IncrementInlineEditorDrawerDepth = true;

        /// <summary>
        /// How the InlineEditor attribute drawer should draw the object field.
        /// </summary>
        public InlineEditorObjectFieldModes ObjectFieldMode;

        /// <summary>
        /// Whether to set GUI.enabled = false when drawing an editor for an asset that is locked by source control. Defaults to true.
        /// </summary>
        public bool DisableGUIForVCSLockedAssets = true;

        public bool ExpandedHasValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineEditorAttribute" /> class.
        /// </summary>
        /// <param name="inlineEditorMode">The inline editor mode.</param>
        /// <param name="objectFieldMode">How the object field should be drawn.</param>
        public InlineEditorAttribute(InlineEditorModes inlineEditorMode = InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes objectFieldMode = InlineEditorObjectFieldModes.Boxed)
        {
            this.ObjectFieldMode = objectFieldMode;

            switch (inlineEditorMode)
            {
                case InlineEditorModes.GUIOnly:
                    this.DrawGUI = true;
                    break;

                case InlineEditorModes.GUIAndHeader:
                    this.DrawGUI = true;
                    this.DrawHeader = true;
                    break;

                case InlineEditorModes.GUIAndPreview:
                    this.DrawGUI = true;
                    this.DrawPreview = true;
                    break;

                case InlineEditorModes.SmallPreview:
                    this.expanded = true;
                    this.DrawPreview = true;
                    break;

                case InlineEditorModes.LargePreview:
                    this.expanded = true;
                    this.DrawPreview = true;
                    this.PreviewHeight = 170;
                    break;

                case InlineEditorModes.FullEditor:
                    this.DrawGUI = true;
                    this.DrawHeader = true;
                    this.DrawPreview = true;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineEditorAttribute"/> class.
        /// </summary>
        /// <param name="objectFieldMode">How the object field should be drawn.</param>
        public InlineEditorAttribute(InlineEditorObjectFieldModes objectFieldMode)
            : this(InlineEditorModes.GUIOnly, objectFieldMode)
        {
        }
    }
}