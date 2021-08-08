#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EventExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Collection of extension methods for <see cref="UnityEngine.Event"/>.
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Returns <c>true</c> when the user presses the specified mouse button.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <param name="mouseButton">The mouse button the user has to press.</param>
        /// <param name="useEvent">If <c>true</c> then the method will call <see cref="UnityEngine.Event.Use"/> on the event.</param>
        /// <returns><c>true</c> on mouse down events with the specified button. Otherwise <c>false</c>.</returns>
        public static bool OnMouseDown(this Event current, int mouseButton, bool useEvent = true)
        {
            var result = current.type == EventType.MouseDown && current.button == mouseButton;
            if (result && useEvent)
            {
                current.Use();
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> when the user clicks a rect with the mouse.
        /// </summary>
        /// <param name="current">The event.</param>
        /// <param name="rect">The rect the user can click on.</param>
        /// <param name="mouseButton">The button the user has to press.</param>
        /// <param name="useEvent">If <c>true</c> then the method will call <see cref="UnityEngine.Event.Use"/> on the event.</param>
        /// <returns><c>true</c> on mouse down events with the specified button. Otherwise <c>false</c>.</returns>
        public static bool OnMouseDown(this Event current, Rect rect, int mouseButton, bool useEvent = true)
        {
            bool result = current.type == EventType.MouseDown && current.button == mouseButton && rect.Contains(current.mousePosition);
            if (result && useEvent)
            {
                current.Use();
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> when the user releases the specified mouse button.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <param name="mouseButton">The mouse button the user has to release.</param>
        /// <param name="useEvent">If <c>true</c> then the method will call <see cref="UnityEngine.Event.Use"/> on the event.</param>
        /// <returns><c>true</c> on mouse up events, with the specified button. Otherwise <c>false</c>.</returns>
        public static bool OnMouseUp(this Event current, int mouseButton, bool useEvent = true)
        {
            var result = current.type == EventType.MouseUp && current.button == mouseButton;
            if (result && useEvent)
            {
                current.Use();
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> when the user releases the specified mouse button over the specified rect.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <param name="rect">The rect the user has to release the mouse button over.</param>
        /// <param name="mouseButton">The mouse button the user has to release.</param>
        /// <param name="useEvent">If <c>true</c> then the method will call <see cref="UnityEngine.Event.Use"/> on the event.</param>
        /// <returns><c>true</c> on mouse up events, with the specified button and over the specified rect. Otherwise <c>false</c>.</returns>
        public static bool OnMouseUp(this Event current, Rect rect, int mouseButton, bool useEvent = true)
        {
            bool result = current.type == EventType.MouseUp && current.button == mouseButton && rect.Contains(current.mousePosition);
            if (result && useEvent)
            {
                current.Use();
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> when the user left clicks a rect.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <param name="rect">The rect the user can click.</param>
        /// <param name="useEvent">If <c>true</c> then the method will call <see cref="UnityEngine.Event.Use"/> on the event.</param>
        /// <returns><c>true</c> on left click events, on the specified rect. Otherwise <c>false</c>.</returns>
        public static bool OnLeftClick(this Event current, Rect rect, bool useEvent = true)
        {
            return EventExtensions.OnMouseDown(current, rect, 0, useEvent);
        }

        /// <summary>
        /// Returns <c>true</c> when the user right clicks a rect.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <param name="rect">The rect the user can right click.</param>
        /// <param name="useEvent">If <c>true</c> then the method will call <see cref="UnityEngine.Event.Use"/> on the event.</param>
        /// <returns><c>true</c> on context click events, on the specified rect. Otherwise <c>false</c>.</returns>
        public static bool OnContextClick(this Event current, Rect rect, bool useEvent = true)
        {
            var result = current.type == EventType.ContextClick && rect.Contains(current.mousePosition);
            if (result && useEvent)
            {
                current.Use();
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> when the user presses the specified key.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <param name="key">The key the user has to press.</param>
        /// <param name="useEvent">If <c>true</c> then the method will call <see cref="UnityEngine.Event.Use"/> on the event.</param>
        /// <returns><c>true</c> on key down events with the specified key code. Otherwise <c>false</c>.</returns>
        public static bool OnKeyDown(this Event current, KeyCode key, bool useEvent = true)
        {
            var result = current.type == EventType.KeyDown && current.keyCode == key;
            if (result && useEvent)
            {
                current.Use();
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> when the user releases the specified key.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <param name="key">The key the user has to release.</param>
        /// <param name="useEvent">If <c>true</c> then the method will call <see cref="UnityEngine.Event.Use"/> on the event.</param>
        /// <returns><c>true</c> on key up events with the specified key code. Otherwise <c>false</c>.</returns>
        public static bool OnKeyUp(this Event current, KeyCode key, bool useEvent = true)
        {
            var result = current.type == EventType.KeyUp && current.keyCode == key;
            if (result && useEvent)
            {
                current.Use();
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> whene the user moves or drags the mouse.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <param name="useEvent">If <c>true</c> then the method will call <see cref="UnityEngine.Event.Use"/> on the event.</param>
        /// <returns><c>true</c> on mouse move or mouse drag events. Otherwise <c>false</c>.</returns>
        public static bool OnMouseMoveDrag(this Event current, bool useEvent = true)
        {
            var result = current.type == EventType.MouseMove || current.type == EventType.MouseDrag;
            if (result && useEvent)
            {
                current.Use();
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> when the user hovers the mouse over the specified rect.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <param name="rect">The rect the user can hover.</param>
        /// <returns><c>true</c> on any event where the mouse is hovering the specified rect. Otherwise <c>false</c>.</returns>
        public static bool IsHovering(this Event current, Rect rect)
        {
            return rect.Contains(current.mousePosition);
        }

        /// <summary>
        /// Returns <c>true</c> on repaint events.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <returns><c>true</c> on repaint events. Otherwise <c>false</c>.</returns>
        public static bool OnRepaint(this Event current)
        {
            return current.type == EventType.Repaint;
        }

        /// <summary>
        /// Returns <c>true</c> on layout events.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <returns><c>true</c> on layout events. Otherwise <c>false</c>.</returns>
        public static bool OnLayout(this Event current)
        {
            return current.type == EventType.Layout;
        }

        /// <summary>
        /// Returns <c>true</c> on the specified event.
        /// </summary>
        /// <param name="current">The <see cref="UnityEngine.Event"/>.</param>
        /// <param name="eventType">The required event type.</param>
        /// <returns><c>true</c> on the specified event. Otherwise <c>false</c>.</returns>
        public static bool OnEventType(this Event current, EventType eventType)
        {
            return current.type == eventType;
        }
    }
}
#endif