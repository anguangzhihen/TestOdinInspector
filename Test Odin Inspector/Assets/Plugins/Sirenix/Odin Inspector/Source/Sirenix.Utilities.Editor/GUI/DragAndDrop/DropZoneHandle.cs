#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DropZoneHandle.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// This class is due to undergo refactoring.
    /// </summary>
    public sealed class DropZoneHandle
    {
        private static DropZoneHandle hoveringDropZone = null;
        private Rect screenRect;
        private bool isBeingHovered;
        private Vector2 mousePosition;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IsAccepted { get; private set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IsBeingHovered
        {
            get
            {
                return this.isBeingHovered && DragAndDropManager.IsDragInProgress;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        internal EditorWindow SourceWindow;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public Rect Rect { get; set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public Type Type { get; internal set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public int LayoutDepth { get; set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool CanAcceptMove { get; internal set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IsCrossWindowDrag { get; private set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IsReadyToClaim
        {
            get
            {
                return
                    this.IsAccepted &&
                    hoveringDropZone == this &&
                    DragAndDropManager.IsDragInProgress &&
                    DragAndDropManager.CurrentDraggingHandle.IsReadyToBeClaimed &&
                    DragAndDropManager.CurrentDraggingHandle.IsBeingClaimed == false &&
                    Event.current.type == EventType.Repaint;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public object ClaimObject()
        {
            if (this.IsReadyToClaim == false)
            {
                throw new Exception("Check IsReadyToClaim before claiming the object.");
            }

            return DragAndDropManager.CurrentDraggingHandle.DropObject();
        }

        internal void Update(EventType eventType)
        {
            if (DragAndDropManager.IsDragInProgress && this.Enabled && DragAndDropManager.AllowDrop && GUI.enabled)
            {
                if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
                {
                    this.mousePosition = GUIHelper.MouseScreenPosition;
                    var rect = this.Rect;

                    var editorWindowOffset = GUIHelper.EditorScreenPointOffset;
                    rect.x += editorWindowOffset.x;
                    rect.y += editorWindowOffset.y;
                    this.screenRect = rect;
                }

                this.IsAccepted = DragAndDropManager.IsDragInProgress &&
                                 (DragAndDropManager.CurrentDraggingHandle.Object != null &&
                                  DragAndDropManager.CurrentDraggingHandle.Object.GetType().InheritsFrom(this.Type) ||
                                  this.Type.IsNullableType() && object.ReferenceEquals(null, DragAndDropManager.CurrentDraggingHandle.Object));

                if (Event.current.type == eventType)
                {
                    if (this.screenRect.Contains(this.mousePosition) && GUIHelper.CurrentWindowHasFocus)
                    {
                        bool setHoveringDropZone =
                            hoveringDropZone == null ||
                            this.LayoutDepth >= hoveringDropZone.LayoutDepth ||
                            hoveringDropZone.screenRect.Contains(this.mousePosition) == false;

                        if (setHoveringDropZone && this.IsAccepted)
                        {
                            hoveringDropZone = this;
                        }

                        if (this.IsAccepted && hoveringDropZone == this)
                        {
                            DragAndDropManager.CurrentHoveringDropZone = this;
                        }
                        else
                        {
                            this.isBeingHovered = false;
                        }
                    }
                    else if (DragAndDropManager.CurrentHoveringDropZone == this)
                    {
                        this.isBeingHovered = false;
                        DragAndDropManager.CurrentHoveringDropZone = null;
                    }

                    this.isBeingHovered = DragAndDropManager.CurrentHoveringDropZone == this;
                }

                if (this.isBeingHovered)
                {
                    this.IsCrossWindowDrag = DragAndDropManager.CurrentDraggingHandle.SourceWindow != this.SourceWindow;
                    DragAndDropManager.CurrentDraggingHandle.IsCrossWindowDrag = this.IsCrossWindowDrag;
                }
            }
            else if (Event.current.type == eventType)
            {
                if (hoveringDropZone == this)
                {
                    hoveringDropZone = null;
                }
                this.isBeingHovered = false;
                this.IsAccepted = false;
                this.IsCrossWindowDrag = false;
            }
        }
    }
}
#endif