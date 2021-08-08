//-----------------------------------------------------------------------
// <copyright file="ButtonAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>Buttons are used on functions, and allows for clickable buttons in the inspector.</para>
    /// </summary>
	/// <example>
    /// <para>The following example shows a component that has an initialize method, that can be called from the inspector.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
	/// {
	///		[Button]
	///		private void Init()
	///		{
	///			// ...
	///		}
	/// }
    /// </code>
    /// </example>
	/// <example>
    /// <para>The following example show how a Button could be used to test a function.</para>
    /// <code>
    /// public class MyBot : MonoBehaviour
	/// {
	///		[Button]
	///		private void Jump()
	///		{
	///			// ...
	///		}
	/// }
    /// </code>
    /// </example>
	/// <example>
	/// <para>The following example show how a Button can named differently than the function it's been attached to.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		[Button("Function")]
	///		private void MyFunction()
	///		{
	///			// ...
	///		}
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="InlineButtonAttribute"/>
	/// <seealso cref="ButtonGroupAttribute"/>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ButtonAttribute : ShowInInspectorAttribute
    {
        /// <summary>
        /// Gets the height of the button. If it's zero or below then use default.
        /// </summary>
        public int ButtonHeight;

        /// <summary>
        /// Use this to override the label on the button.
        /// </summary>
        public string Name;

        /// <summary>
        /// The style in which to draw the button.
        /// </summary>
        public ButtonStyle Style;

        /// <summary>
        /// If the button contains parameters, you can disable the foldout it creates by setting this to true.
        /// </summary>
        public bool Expanded;

        /// <summary>
        /// <para>Whether to display the button method's parameters (if any) as values in the inspector. True by default.</para>
        /// <para>If this is set to false, the button method will instead be invoked through an ActionResolver or ValueResolver (based on whether it returns a value), giving access to contextual named parameter values like "InspectorProperty property" that can be passed to the button method.</para>
        /// </summary>
        public bool DisplayParameters = true;

        /// <summary>
        /// If the button has a return type, set this to false to not draw the result. Default value is true.
        /// </summary>
        public bool DrawResult
        {
            set
            {
                this.drawResult = value;
                this.drawResultIsSet = true;
            }
            get { return this.drawResult; }
        }

        public bool DrawResultIsSet
        {
            get { return this.drawResultIsSet; }
        }

        private bool drawResult;
        private bool drawResultIsSet;

        /// <summary>
        /// Creates a button in the inspector named after the method.
        /// </summary>
        public ButtonAttribute()
        {
            this.Name = null;
            this.ButtonHeight = (int)ButtonSizes.Small;
        }

        /// <summary>
        /// Creates a button in the inspector named after the method.
        /// </summary>
        /// <param name="size">The size of the button.</param>
        public ButtonAttribute(ButtonSizes size)
        {
            this.Name = null;
            this.ButtonHeight = (int)size;
        }

        /// <summary>
        /// Creates a button in the inspector named after the method.
        /// </summary>
        /// <param name="buttonSize">The size of the button.</param>
        public ButtonAttribute(int buttonSize)
        {
            this.ButtonHeight = buttonSize;
            this.Name = null;
        }

        /// <summary>
        /// Creates a button in the inspector with a custom name.
        /// </summary>
        /// <param name="name">Custom name for the button.</param>
        public ButtonAttribute(string name)
        {
            this.Name = name;
            this.ButtonHeight = (int)ButtonSizes.Small;
        }

        /// <summary>
        /// Creates a button in the inspector with a custom name.
        /// </summary>
        /// <param name="name">Custom name for the button.</param>
        /// <param name="buttonSize">Size of the button.</param>
        public ButtonAttribute(string name, ButtonSizes buttonSize)
        {
            this.Name = name;
            this.ButtonHeight = (int)buttonSize;
        }

        /// <summary>
        /// Creates a button in the inspector with a custom name.
        /// </summary>
        /// <param name="name">Custom name for the button.</param>
        /// <param name="buttonSize">Size of the button in pixels.</param>
        public ButtonAttribute(string name, int buttonSize)
        {
            this.Name = name;
            this.ButtonHeight = buttonSize;
        }

        /// <summary>
        /// Creates a button in the inspector named after the method.
        /// </summary>
        /// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
        public ButtonAttribute(ButtonStyle parameterBtnStyle)
        {
            this.Name = null;
            this.ButtonHeight = (int)ButtonSizes.Small;
            this.Style = parameterBtnStyle;
        }

        /// <summary>
        /// Creates a button in the inspector named after the method.
        /// </summary>
        /// <param name="buttonSize">The size of the button.</param>
        /// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
        public ButtonAttribute(int buttonSize, ButtonStyle parameterBtnStyle)
        {
            this.ButtonHeight = buttonSize;
            this.Name = null;
            this.Style = parameterBtnStyle;
        }

        /// <summary>
        /// Creates a button in the inspector named after the method.
        /// </summary>
        /// <param name="size">The size of the button.</param>
        /// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
        public ButtonAttribute(ButtonSizes size, ButtonStyle parameterBtnStyle)
        {
            this.ButtonHeight = (int)size;
            this.Name = null;
            this.Style = parameterBtnStyle;
        }

        /// <summary>
        /// Creates a button in the inspector with a custom name.
        /// </summary>
        /// <param name="name">Custom name for the button.</param>
        /// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
        public ButtonAttribute(string name, ButtonStyle parameterBtnStyle)
        {
            this.Name = name;
            this.ButtonHeight = (int)ButtonSizes.Small;
            this.Style = parameterBtnStyle;
        }

        /// <summary>
        /// Creates a button in the inspector with a custom name.
        /// </summary>
        /// <param name="name">Custom name for the button.</param>
        /// <param name="buttonSize">Size of the button.</param>
        /// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
        public ButtonAttribute(string name, ButtonSizes buttonSize, ButtonStyle parameterBtnStyle)
        {
            this.Name = name;
            this.ButtonHeight = (int)buttonSize;
            this.Style = parameterBtnStyle;
        }

        /// <summary>
        /// Creates a button in the inspector with a custom name.
        /// </summary>
        /// <param name="name">Custom name for the button.</param>
        /// <param name="buttonSize">Size of the button in pixels.</param>
        /// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
        public ButtonAttribute(string name, int buttonSize, ButtonStyle parameterBtnStyle)
        {
            this.Name = name;
            this.ButtonHeight = buttonSize;
            this.Style = parameterBtnStyle;
        }
    }
}