#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DefaultDrawerChainResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.TypeSearch;
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;

    public class DefaultDrawerChainResolver : DrawerChainResolver
    {
        public static readonly DefaultDrawerChainResolver Instance = new DefaultDrawerChainResolver();

        private static readonly Dictionary<Type, Func<OdinDrawer>> FastDrawerCreators = new Dictionary<Type, Func<OdinDrawer>>(FastTypeComparer.Instance);
        private static TypeSearchResult[] CachedResultArray = new TypeSearchResult[20];

        public override DrawerChain GetDrawerChain(InspectorProperty property)
        {
            List<OdinDrawer> drawers = new List<OdinDrawer>(10);

            int resultCount = 0;
            DrawerUtilities.GetDefaultPropertyDrawers(property, ref CachedResultArray, ref resultCount);

            for (int i = 0; i < resultCount; i++)
            {
                drawers.Add(CreateDrawer(CachedResultArray[i].MatchedType));
            }

            return new ListDrawerChain(property, drawers);
        }

        private static OdinDrawer CreateDrawer(Type drawerType)
        {
            Func<OdinDrawer> fastCreator;

            if (!FastDrawerCreators.TryGetValue(drawerType, out fastCreator))
            {
                var constructor = drawerType.GetConstructor(Type.EmptyTypes);
                var method = new DynamicMethod(drawerType.FullName + "_FastCreator", typeof(OdinDrawer), Type.EmptyTypes);

                var il = method.GetILGenerator();

                il.Emit(OpCodes.Newobj, constructor);
                il.Emit(OpCodes.Ret);

                fastCreator = (Func<OdinDrawer>)method.CreateDelegate(typeof(Func<OdinDrawer>));
                FastDrawerCreators.Add(drawerType, fastCreator);
            }

            return fastCreator();
        }
    }
}
#endif