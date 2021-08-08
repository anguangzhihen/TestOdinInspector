#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InvalidAttributeNotificationDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    [DrawerPriority(-1, -1, -1), OdinDontRegister]
    public class InvalidAttributeNotificationDrawer<TInvalidAttribute> : OdinDrawer
    {
        private class Context
        {
            public string ErrorMessage;
            public string ValidTypeMessage;
            public bool IsFolded = true;
        }

        private Context context;

        protected override void Initialize()
        {
            this.context = new Context();

            var sb = new StringBuilder("Attribute '")
                .Append(typeof(TInvalidAttribute).GetNiceName())
                .Append("' cannot be put on property '")
                .Append(Property.Name)
                .Append("'");

            if (Property.ValueEntry != null)
            {
                sb.Append(" of base type '")
                  .Append(Property.ValueEntry.BaseValueType.GetNiceName())
                  .Append("'");
            }
            sb.Append('.');

            context.ErrorMessage = sb.ToString();

            sb.Length = 0;

            var validTypes = DrawerUtilities.InvalidAttributeTargetUtility.GetValidTargets(typeof(TInvalidAttribute));
            sb.AppendLine("The following types are valid:");
            sb.AppendLine();

            for (int i = 0; i < validTypes.Count; i++)
            {
                var type = validTypes[i];
                sb.Append(type.GetNiceName());

                if (type.IsGenericParameter)
                {
                    sb.Append(" ")
                      .Append(type.GetGenericParameterConstraintsString(useFullTypeNames: true));
                }

                sb.AppendLine();
            }

            sb.Append("Supported collections where the element type is any of the above types");

            context.ValidTypeMessage = sb.ToString();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            context.IsFolded = SirenixEditorGUI.DetailedMessageBox(context.ErrorMessage, context.ValidTypeMessage, MessageType.Error, context.IsFolded);

            this.CallNextDrawer(label);
        }
    }
}
#endif