//-----------------------------------------------------------------------
// <copyright file="TitleAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>Title is used to make a bold header above a property.</para>
    /// </summary>
    /// <example>
    /// The following example shows how Title is used on different properties.
    /// <code>
    /// public class TitleExamples : MonoBehaviour
    /// {
    ///     [Title("Titles and Headers")]
    ///     [InfoBox(
    ///         "The Title attribute has the same purpose as Unity's Header attribute," +
    ///         "but it also supports properties, and methods." +
    ///         "\n\nTitle also offers more features such as subtitles, options for horizontal underline, bold text and text alignment." +
    ///         "\n\nBoth attributes, with Odin, supports either static strings, or refering to members strings by adding a $ in front.")]
    ///     public string MyTitle = "My Dynamic Title";
    ///     public string MySubtitle = "My Dynamic Subtitle";
    /// 
    ///     [Title("Static title")]
    ///     public int C;
    ///     public int D;
    /// 
    ///     [Title("Static title", "Static subtitle")]
    ///     public int E;
    ///     public int F;
    /// 
    ///     [Title("$MyTitle", "$MySubtitle")]
    ///     public int G;
    ///     public int H;
    /// 
    ///     [Title("Non bold title", "$MySubtitle", bold: false)]
    ///     public int I;
    ///     public int J;
    /// 
    ///     [Title("Non bold title", "With no line seperator", horizontalLine: false, bold: false)]
    ///     public int K;
    ///     public int L;
    /// 
    ///     [Title("$MyTitle", "$MySubtitle", TitleAlignments.Right)]
    ///     public int M;
    ///     public int N;
    /// 
    ///     [Title("$MyTitle", "$MySubtitle", TitleAlignments.Centered)]
    ///     public int O;
    ///     public int P;
    /// 
    ///     [Title("$Combined", titleAlignment: TitleAlignments.Centered)]
    ///     public int Q;
    ///     public int R;
    /// 
    ///     [ShowInInspector]
    ///     [Title("Title on a Property")]
    ///     public int S { get; set; }
    /// 
    ///     [Title("Title on a Method")]
    ///     [Button]
    ///     public void DoNothing()
    ///     { }
    /// 
    ///     public string Combined { get { return this.MyTitle + " - " + this.MySubtitle; } }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ButtonAttribute"/>
    /// <seealso cref="LabelTextAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class TitleAttribute : Attribute
    {
        /// <summary>
        /// The title displayed above the property in the inspector.
        /// </summary>
        public string Title;

        /// <summary>
        /// Optional subtitle.
        /// </summary>
		public string Subtitle;

        /// <summary>
        /// If <c>true</c> the title will be displayed with a bold font.
        /// </summary>
        public bool Bold;

        /// <summary>
        /// Gets a value indicating whether or not to draw a horizontal line below the title.
        /// </summary>
		public bool HorizontalLine;

        /// <summary>
        /// Title alignment.
        /// </summary>
        public TitleAlignments TitleAlignment;

        /// <summary>
        /// Creates a title above any property in the inspector.
        /// </summary>
        /// <param name="title">The title displayed above the property in the inspector.</param>
        /// <param name="subtitle">Optional subtitle</param>
        /// <param name="titleAlignment">Title alignment</param>
        /// <param name="horizontalLine">Horizontal line</param>
        /// <param name="bold">If <c>true</c> the title will be drawn with a bold font.</param>
        public TitleAttribute(string title, string subtitle = null, TitleAlignments titleAlignment = TitleAlignments.Left, bool horizontalLine = true, bool bold = true)
        {
            this.Title = title ?? "null";
            this.Subtitle = subtitle;
            this.Bold = bold;
            this.TitleAlignment = titleAlignment;
            this.HorizontalLine = horizontalLine;
        }
	}
}