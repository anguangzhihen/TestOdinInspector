#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValidationResult.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using System;

    public class ValidationResult
    {
        public string Message;
        public ValidationResultType ResultType;
        public object ResultValue;
        public ValidationSetup Setup;
        public string Path;

        [Obsolete("Use Path instead.", false)]
        public string GetFullPath()
        {
            return this.Path;
        }

        public void RerunValidation()
        {
            if (this.Setup.Validator == null)
                return;

            var result = this;
            var setupBackup = this.Setup;

            try
            {
                this.Setup.Validator.RunValidation(ref result);
            }
            catch (Exception ex)
            {
                this.Setup = setupBackup;

                this.Message = "An exception was thrown during validation of property " + this.Setup.Validator.Property.Path + ": " + ex.ToString();
                this.ResultType = ValidationResultType.Error;
            }
        }

        public ValidationResult CreateCopy()
        {
            var copy = new ValidationResult();

            copy.Path = this.Path;
            copy.Message = this.Message;
            copy.ResultType = this.ResultType;
            copy.ResultValue = this.ResultValue;
            copy.Setup = this.Setup;

            return copy;
        }
    }
}
#endif