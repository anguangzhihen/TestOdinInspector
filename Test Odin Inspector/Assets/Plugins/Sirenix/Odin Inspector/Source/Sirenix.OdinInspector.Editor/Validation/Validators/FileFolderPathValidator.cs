#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FileFolderPathValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.FilePathValidator))]
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.FolderPathValidator))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using System.IO;

    public sealed class FilePathValidator : AttributeValidator<FilePathAttribute, string>
    {
        private bool requireExistingPath;
        private ValueResolver<string> parentPathProvider;

        protected override void Initialize()
        {
            this.requireExistingPath = this.Attribute.RequireExistingPath;

            if (this.requireExistingPath)
            {
                this.parentPathProvider = ValueResolver.GetForString(this.Property, this.Attribute.ParentFolder);
            }
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.requireExistingPath)
            {
                string path = this.ValueEntry.SmartValue ?? string.Empty;
                string parent = this.parentPathProvider.GetValue() ?? string.Empty;

                if (string.IsNullOrEmpty(parent) == false)
                {
                    path = Path.Combine(parent, path);
                }

                if (File.Exists(path))
                {
                    result.ResultType = ValidationResultType.Valid;
                }
                else
                {
                    result.ResultType = ValidationResultType.Error;
                    result.Message = "The path does not exist.";
                }
            }
            else
            {
                result.ResultType = ValidationResultType.IgnoreResult;
            }
        }
    }

    public sealed class FolderPathValidator : AttributeValidator<FolderPathAttribute, string>
    {
        private bool requireExistingPath;
        private ValueResolver<string> parentPathProvider;

        protected override void Initialize()
        {
            this.requireExistingPath = this.Attribute.RequireExistingPath;

            if (this.requireExistingPath)
            {
                this.parentPathProvider = ValueResolver.GetForString(this.Property, this.Attribute.ParentFolder);
            }
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.requireExistingPath)
            {
                string path = this.ValueEntry.SmartValue ?? string.Empty;
                string parent = this.parentPathProvider.GetValue() ?? string.Empty;

                if (string.IsNullOrEmpty(parent) == false)
                {
                    path = Path.Combine(parent, path);
                }

                if (Directory.Exists(path.TrimEnd('/', '\\') + "/"))
                {
                    result.ResultType = ValidationResultType.Valid;
                }
                else
                {
                    result.ResultType = ValidationResultType.Error;
                    result.Message = "The path does not exist.";
                }
            }
            else
            {
                result.ResultType = ValidationResultType.IgnoreResult;
            }
        }
    }
}
#endif