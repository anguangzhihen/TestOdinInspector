#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OnValueChangedAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEngine;
    using System;
    using Sirenix.OdinInspector.Editor.ActionResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="OnValueChangedAttribute"/>.
    /// </summary>
    /// <seealso cref="OnCollectionChangedAttribute"/>
    /// <seealso cref="OnValueChangedAttribute"/>
    /// <seealso cref="OnInspectorGUIAttribute"/>
    /// <seealso cref="ValidateInputAttribute"/>
    /// <seealso cref="InfoBoxAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class OnValueChangedAttributeDrawer<T> : OdinAttributeDrawer<OnValueChangedAttribute, T>, IDisposable
    {
        private ActionResolver onChangeAction;
        private bool subscribedToOnUndoRedo;

        protected override void Initialize()
        {
            if (this.Attribute.InvokeOnUndoRedo)
            {
                this.Property.Tree.OnUndoRedoPerformed += this.OnUndoRedo;
                this.subscribedToOnUndoRedo = true;
            }

            this.onChangeAction = ActionResolver.Get(this.Property, this.Attribute.Action);

            // TODO: OnValueChanged notifications whenever prefabs are modified, if this is the modified prefab
            //Undo.postprocessModifications += TODO_THIS;

            Action<int> triggerAction = this.TriggerAction;
            this.ValueEntry.OnValueChanged += triggerAction;

            if (this.Attribute.IncludeChildren || typeof(T).IsValueType)
            {
                this.ValueEntry.OnChildValueChanged += triggerAction;
            }

			if (this.Attribute.InvokeOnInitialize && !this.onChangeAction.HasError)
			{
				this.onChangeAction.DoActionForAllSelectionIndices();
			}
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.onChangeAction.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.onChangeAction.ErrorMessage);
            }

            this.CallNextDrawer(label);
        }

        private void OnUndoRedo()
        {
            for (int i = 0; i < this.ValueEntry.ValueCount; i++)
            {
                this.TriggerAction(i);
            }
        }

        private void TriggerAction(int selectionIndex)
        {
            this.onChangeAction.DoAction(selectionIndex);
        }

        public void Dispose()
        {
            if (this.subscribedToOnUndoRedo)
            {
                this.Property.Tree.OnUndoRedoPerformed -= this.OnUndoRedo;
            }
        }
    }
}
#endif