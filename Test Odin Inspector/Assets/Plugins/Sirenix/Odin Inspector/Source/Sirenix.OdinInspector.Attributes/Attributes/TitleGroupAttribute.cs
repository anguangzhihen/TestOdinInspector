//-----------------------------------------------------------------------
// <copyright file="TitleGroupAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

	using System;

    /// <summary>
    /// Groups properties vertically together with a title, an optional subtitle, and an optional horizontal line. 
    /// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public sealed class TitleGroupAttribute : PropertyGroupAttribute
	{
        /// <summary>
        /// Optional subtitle.
        /// </summary>
		public string Subtitle;

        /// <summary>
        /// Title alignment.
        /// </summary>
        public TitleAlignments Alignment;

        /// <summary>
        /// Gets a value indicating whether or not to draw a horizontal line below the title.
        /// </summary>
        public bool HorizontalLine;

        /// <summary>
        /// If <c>true</c> the title will be displayed with a bold font.
        /// </summary>
        public bool BoldTitle;

        /// <summary>
        /// Gets a value indicating whether or not to indent all group members.
        /// </summary>
        public bool Indent;

        /// <summary>
        /// Groups properties vertically together with a title, an optional subtitle, and an optional horizontal line. 
        /// </summary>
        /// <param name="title">The title-</param>
        /// <param name="subtitle">Optional subtitle.</param>
        /// <param name="alignment">The text alignment.</param>
        /// <param name="horizontalLine">Horizontal line.</param>
        /// <param name="boldTitle">Bold text.</param>
        /// <param name="indent">Whether or not to indent all group members.</param>
        /// <param name="order">The group order.</param>
		public TitleGroupAttribute(string title, string subtitle = null, TitleAlignments alignment = TitleAlignments.Left, bool horizontalLine = true, bool boldTitle = true, bool indent = false, float order = 0) : base(title, order)
		{
			this.Subtitle = subtitle;
			this.Alignment = alignment;
			this.HorizontalLine = horizontalLine;
			this.BoldTitle = boldTitle;
			this.Indent = indent;
		}

		/// <summary>
		/// Combines TitleGroup attributes.
		/// </summary>
        /// <param name="other">The other group attribute to combine with.</param>
		protected override void CombineValuesWith(PropertyGroupAttribute other)
		{
			var t = other as TitleGroupAttribute;

			if (this.Subtitle != null)
			{
				t.Subtitle = this.Subtitle;
			}
			else
			{
				this.Subtitle = t.Subtitle;
			}

			if (this.Alignment != TitleAlignments.Left)
			{
				t.Alignment = this.Alignment;
			}
			else
			{
				this.Alignment = t.Alignment;
			}

			if (this.HorizontalLine != true)
			{
				t.HorizontalLine = this.HorizontalLine;
			}
			else
			{
				this.HorizontalLine = t.HorizontalLine;
			}

			if (this.BoldTitle != true)
			{
				t.BoldTitle = this.BoldTitle;
			}
			else
			{
				this.BoldTitle = t.BoldTitle;
			}

			if (this.Indent == true)
			{
				t.Indent = this.Indent;
			}
			else
			{
				this.Indent = t.Indent;
			}
		}
	}
}