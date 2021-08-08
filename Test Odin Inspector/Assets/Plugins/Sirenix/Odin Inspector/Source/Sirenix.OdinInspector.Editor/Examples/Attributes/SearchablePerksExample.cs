#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SearchablePerksExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using System;
    using System.Collections.Generic;

    [AttributeExample(typeof(SearchableAttribute), "The Searchable attribute can be applied to individual members in a type, to make only that member searchable.", Order = -2)]
    [ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic" })]
    internal class SearchablePerksExample
    {
        [Searchable]
        public List<Perk> Perks = new List<Perk>()
        {
            new Perk()
            {
                Name = "Old Sage",
                Effects = new List<Effect>()
                {
                    new Effect() { Skill = Skill.Wisdom, Value = 2, },
                    new Effect() { Skill = Skill.Intelligence, Value = 1, },
                    new Effect() { Skill = Skill.Strength, Value = -2 },
                },
            },
            new Perk()
            {
                Name = "Hardened Criminal",
                Effects = new List<Effect>()
                {
                    new Effect() { Skill = Skill.Dexterity, Value = 2, },
                    new Effect() { Skill = Skill.Strength, Value = 1, },
                    new Effect() { Skill = Skill.Charisma, Value = -2 },
                },
            },
            new Perk()
            {
                Name = "Born Leader",
                Effects = new List<Effect>()
                {
                    new Effect() { Skill = Skill.Charisma, Value = 2, },
                    new Effect() { Skill = Skill.Intelligence, Value = -3 },
                },
            },
            new Perk()
            {
                Name = "Village Idiot",
                Effects = new List<Effect>()
                {
                    new Effect() { Skill = Skill.Charisma, Value = 4, },
                    new Effect() { Skill = Skill.Constitution, Value = 2, },
                    new Effect() { Skill = Skill.Intelligence, Value = -3 },
                    new Effect() { Skill = Skill.Wisdom, Value = -3 },
                },
            },
        };

        [Serializable]
        public class Perk
        {
            public string Name;

            [TableList]
            public List<Effect> Effects;
        }

        [Serializable]
        public class Effect
        {
            public Skill Skill;
            public float Value;
        }

        public enum Skill
        {
            Strength,
            Dexterity,
            Constitution,
            Intelligence,
            Wisdom,
            Charisma,
        }
    }
}
#endif