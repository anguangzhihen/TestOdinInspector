#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValidationDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [DrawerPriority(0, 10000, 0)]
    public class ValidationDrawer<T> : OdinValueDrawer<T>, IDisposable
    {
        private List<ValidationResult> validationResults;
        private bool rerunFullValidation;
        private object shakeGroupKey;

        private ValidationComponent validationComponent;
        
        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            var validation = property.GetComponent<ValidationComponent>();
            if (validation == null) return false;
            if (property.GetAttribute<DontValidateAttribute>() != null) return false;
            return validation.ValidatorLocator.PotentiallyHasValidatorsFor(property);
        }

        protected override void Initialize()
        {
            this.validationComponent = this.Property.GetComponent<ValidationComponent>();
            this.validationComponent.ValidateProperty(ref this.validationResults);

            if (this.validationResults.Count > 0)
            {
                this.shakeGroupKey = UniqueDrawerKey.Create(this.Property, this);

                this.Property.Tree.OnUndoRedoPerformed += this.OnUndoRedoPerformed;
                this.ValueEntry.OnValueChanged += this.OnValueChanged;
                this.ValueEntry.OnChildValueChanged += this.OnChildValueChanged;
            }
            else
            {
                this.SkipWhenDrawing = true;
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.validationResults.Count == 0)
            {
                this.CallNextDrawer(label);
                return;
            }

            GUILayout.BeginVertical();
            SirenixEditorGUI.BeginShakeableGroup(this.shakeGroupKey);

            for (int i = 0; i < this.validationResults.Count; i++)
            {
                var result = this.validationResults[i];

                if (Event.current.type == EventType.Layout && (this.rerunFullValidation || result.Setup.Validator.RevalidationCriteria == RevalidationCriteria.Always))
                {
                    var formerResultType = result.ResultType;

                    result.Setup.ParentInstance = this.Property.ParentValues[0];
                    result.Setup.Value = this.ValueEntry.Values[0];

                    result.RerunValidation();

                    if (formerResultType != result.ResultType && result.ResultType != ValidationResultType.Valid)
                    {
                        // We got a new result that was not valid
                        SirenixEditorGUI.StartShakingGroup(this.shakeGroupKey);
                    }
                }

                if (result.ResultType == ValidationResultType.Error)
                {
                    SirenixEditorGUI.ErrorMessageBox(result.Message);
                }
                else if (result.ResultType == ValidationResultType.Warning)
                {
                    SirenixEditorGUI.WarningMessageBox(result.Message);
                }
                else if (result.ResultType == ValidationResultType.Valid && !string.IsNullOrEmpty(result.Message))
                {
                    SirenixEditorGUI.InfoMessageBox(result.Message);
                }
            }

            if (Event.current.type == EventType.Layout)
            {
                this.rerunFullValidation = false;
            }

            this.CallNextDrawer(label);
            SirenixEditorGUI.EndShakeableGroup(this.shakeGroupKey);
            GUILayout.EndVertical();
        }

        public void Dispose()
        {
            if (this.validationResults.Count > 0)
            {
                this.Property.Tree.OnUndoRedoPerformed -= this.OnUndoRedoPerformed;
                this.ValueEntry.OnValueChanged -= this.OnValueChanged;
                this.ValueEntry.OnChildValueChanged -= this.OnChildValueChanged;
            }

            this.validationResults = null;
        }

        private void OnUndoRedoPerformed()
        {
            this.rerunFullValidation = true;
        }

        private void OnValueChanged(int index)
        {
            this.rerunFullValidation = true;
        }

        private void OnChildValueChanged(int index)
        {
            this.rerunFullValidation = true;
        }
    }
}
#endif