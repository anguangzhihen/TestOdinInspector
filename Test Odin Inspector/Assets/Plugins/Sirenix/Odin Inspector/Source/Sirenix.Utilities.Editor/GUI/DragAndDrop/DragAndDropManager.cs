#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DragAndDropManager.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// <para>This class is due to undergo refactoring. Use the new DragAndDropUtilities instead.</para>
    /// </summary>
    /// <seealso cref="DragAndDropUtilities"/>
    public static class DragAndDropManager
    {
        private static GUIScopeStack<DragHandle> dragHandles = new GUIScopeStack<DragHandle>();

        private static GUIScopeStack<DropZoneHandle> dropZoneHandles = new GUIScopeStack<DropZoneHandle>();

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static DragHandle CurrentDraggingHandle { get; internal set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static DropZoneHandle CurrentHoveringDropZone { get; internal set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool IsDragInProgress
        {
            get { return CurrentDraggingHandle != null; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool IsHoveringDropZone
        {
            get { return CurrentHoveringDropZone != null; }
        }

        internal static bool WasDragPerformed = false;

        private static object dropZoneKey = new object();

        private static object draggableKey = new object();

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool AllowDrop = true;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static DropZoneHandle BeginDropZone<T>(object key) where T : struct
        {
            return BeginDropZone(key, typeof(T), false);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static DropZoneHandle BeginDropZone<T>(object key, bool canAcceptMove) where T : class
        {
            return BeginDropZone(key, typeof(T), canAcceptMove);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static DropZoneHandle BeginDropZone(object key, Type type, bool canAcceptMove)
        {
            Update();
            GUILayout.BeginVertical();
            var rect = GUIHelper.GetCurrentLayoutRect();
            var dropZoneHandle = GUIHelper.GetTemporaryContext<DropZoneHandle>(dropZoneKey, key).Value;
            dropZoneHandle.Type = type;
            dropZoneHandle.CanAcceptMove = canAcceptMove;
            dropZoneHandle.LayoutDepth = dropZoneHandles.Count;
            dropZoneHandles.Push(dropZoneHandle);
            dropZoneHandle.Update(EventType.Layout);
            dropZoneHandle.SourceWindow = GUIHelper.CurrentWindow;

            if (Event.current.type == EventType.Repaint)
            {
                dropZoneHandle.Rect = rect;
            }

            return dropZoneHandle;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static DropZoneHandle EndDropZone()
        {
            var dropZoneHandle = dropZoneHandles.Pop();
            GUILayout.EndVertical();
            dropZoneHandle.Update(EventType.Repaint);
            return dropZoneHandle;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static DragHandle BeginDragHandle(object key, object obj, DragAndDropMethods defaultMethod = DragAndDropMethods.Move)
        {
            return BeginDragHandle(key, obj, false, defaultMethod);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static DragHandle BeginDragHandle(object key, object obj, bool isVirtualDragHandle, DragAndDropMethods defaultMethod = DragAndDropMethods.Move)
        {
            Update();

            if(Event.current.type == EventType.Repaint)
            {
                GUIHelper.BeginLayoutMeasuring();
            }

            var dragHandle = GUIHelper.GetTemporaryContext<DragHandle>(draggableKey, key).Value;
            dragHandle.Object = obj;
            dragHandle.DragAndDropMethod = defaultMethod;
            dragHandle.LayoutDepth = dragHandles.Count;
            dragHandles.Push(dragHandle);

            dragHandle.SourceWindow = GUIHelper.CurrentWindow;
            return dragHandle;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static DragHandle EndDragHandle()
        {
            var dragHandle = dragHandles.Pop();

            if (Event.current.type == EventType.Repaint)
            {
                var rect = GUIHelper.EndLayoutMeasuring();
                if (dragHandle.IsDragging == false)
                {
                    dragHandle.TempRect = rect;
                }
            }

            dragHandle.Update();
            dragHandle.Rect = dragHandle.TempRect;

            return dragHandle;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        private static GUIFrameCounter guiState = new GUIFrameCounter();

        private static void Update()
        {
            if (guiState.Update().IsNewFrame && IsDragInProgress)
            {
                AllowDrop = true;
                if (IsDragInProgress)
                {
                    GUIHelper.RequestRepaint();
                }
            }

            if (IsDragInProgress)
            {
                // Ensure drop event!
                if (WasDragPerformed == false)
                {
                    if (Event.current.type == EventType.DragPerform ||
                        Event.current.type == EventType.MouseMove ||
                        Event.current.type == EventType.MouseUp)
                    {
                        //Debug.Log(Event.current.type + " - " + Event.current.rawType);
                        WasDragPerformed = true;
                        if (Event.current.type == EventType.DragPerform && IsHoveringDropZone)
                        {
                            Event.current.Use();
                        }
                        GUIHelper.RequestRepaint();
                    }
                }

                if (IsHoveringDropZone && GUIHelper.CurrentWindowHasFocus)
                {
                    if (CurrentHoveringDropZone.IsAccepted == false)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                    else if (CurrentDraggingHandle.CurrentMethod == DragAndDropMethods.Move)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    else if (CurrentDraggingHandle.CurrentMethod == DragAndDropMethods.Reference)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    }
                    else if (CurrentDraggingHandle.CurrentMethod == DragAndDropMethods.Copy)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    }
                }
            }
            else
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    // TODO Start virtual drag.
                }
            }
        }
    }
}
#endif