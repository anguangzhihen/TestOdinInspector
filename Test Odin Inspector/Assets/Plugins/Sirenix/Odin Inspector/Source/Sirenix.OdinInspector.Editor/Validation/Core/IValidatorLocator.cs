#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IValidatorLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IValidatorLocator
    {
        bool PotentiallyHasValidatorsFor(InspectorProperty property);

        [Obsolete("Use PotentiallyHasValidatorsFor(InspectorProperty property) instead.", true)]
        bool PotentiallyHasValidatorsFor(Type valueType);

        [Obsolete("Use PotentiallyHasValidatorsFor(InspectorProperty property) instead.", true)]
        bool PotentiallyHasValidatorsFor(MemberInfo member, Type memberValueType, bool isCollectionElement);

        IList<Validator> GetValidators(InspectorProperty property);

        [Obsolete("Use GetValidators(InspectorProperty property) instead.", true)]
        IList<Validator> GetValidators(Type valueType);

        [Obsolete("Use GetValidators(InspectorProperty property) instead.", true)]
        IList<Validator> GetValidators(MemberInfo member, Type memberValueType, bool isCollectionElement);
    }
}
#endif