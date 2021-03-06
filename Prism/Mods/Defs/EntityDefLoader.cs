﻿using System;
using System.Collections.Generic;
using System.Linq;
using Prism.API;
using Prism.API.Behaviours;
using Prism.API.Defs;
using Terraria;
using Terraria.ID;

namespace Prism.Mods.Defs
{
    /// <summary>
    /// Controls the loading of entities.
    /// </summary>
    static class EntityDefLoader
    {
        /// <summary>
        /// Sets the read-only properties of the entity definitions contained within the dictionary specified,
        /// setting their <see cref="EntityDef.InternalName"/> properties as well as assuring that their <see cref="EntityDef.Mod"/> properties
        /// point to the specified <see cref="ModDef"/>.
        /// </summary>
        /// <typeparam name="TEntityDef"></typeparam>
        /// <param name="def"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        static Dictionary<string, TEntityDef> SetChildReadonlyProperties<TEntityDef, TBehaviour, TEntity>(ModDef def, Dictionary<string, TEntityDef> dict)
            where TEntity : class
            where TBehaviour : EntityBehaviour<TEntity>
            where TEntityDef : EntityDef<TBehaviour, TEntity>
        {
            foreach (var kvp in dict)
            {
                kvp.Value.InternalName = kvp.Key;
                kvp.Value.Mod = def.Info;
            }

            return dict;
        }

        /// <summary>
        /// Resets all the item/NPC/tile/projectile/etc def handlers.
        /// </summary>
        internal static void ResetEntityHandlers()
        {
            ItemDefHandler.Reset();
            NpcDefHandler .Reset();
        }

        /// <summary>
        /// Sets up this EntityDefLoader for loading mods, creating/adding all of the vanilla content defs, etc.
        /// </summary>
        internal static void SetupEntityHandlers()
        {
            ItemDefHandler.FillVanilla();
            NpcDefHandler .FillVanilla();
        }

        /// <summary>
        /// Loads a mod and returns all <see cref="LoaderError"/>s encountered.
        /// </summary>
        /// <param name="mod">The mod to load.</param>
        /// <returns>Enumerable list of LoaderErrors encountered while loading the mod.</returns>
        internal static IEnumerable<LoaderError> Load(ModDef mod)
        {
            var ret = new List<LoaderError>();

            mod.ItemDefs = SetChildReadonlyProperties<ItemDef, ItemBehaviour, Item>(mod, mod.GetItemDefsInternally());
            ret.AddRange(ItemDefHandler.Load(mod.ItemDefs));

            mod.NpcDefs  = SetChildReadonlyProperties<NpcDef , NpcBehaviour , NPC >(mod, mod.GetNpcDefsInternally ());
            ret.AddRange(NpcDefHandler .Load(mod.NpcDefs ));

            return ret;
        }
    }
}
