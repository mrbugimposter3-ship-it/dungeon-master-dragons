using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonMaster
{
    public class Inventory : MonoBehaviour
    {
        public readonly List<ItemId> items = new List<ItemId>();
        public int Attack { get; private set; }
        public int Defence { get; private set; }
        public int FireRes { get; private set; }
        public bool TorchLit { get; private set; }

        public event Action Changed;

        void Awake()
        {
            Recalc();
        }

        public bool Has(ItemId id)
        {
            return items.Contains(id);
        }

        public bool Add(ItemId id)
        {
            if (items.Contains(id)) return false;
            items.Add(id);
            if (id == ItemId.Torch) TorchLit = true;
            Recalc();
            if (Changed != null) Changed();
            return true;
        }

        public bool Remove(ItemId id)
        {
            if (!items.Remove(id)) return false;
            if (id == ItemId.Torch) TorchLit = false;
            Recalc();
            if (Changed != null) Changed();
            return true;
        }

        public void Clear()
        {
            items.Clear();
            TorchLit = false;
            Recalc();
            if (Changed != null) Changed();
        }

        public CombatMath.Profile CombatProfile()
        {
            return CombatMath.Build(Attack, Defence, FireRes);
        }

        void Recalc()
        {
            int attack = CombatMath.BaseAttack;
            int defence = CombatMath.BaseDefence;
            int fire = CombatMath.BaseFireRes;
            for (int i = 0; i < items.Count; i++)
            {
                var def = ItemCatalog.Get(items[i]);
                attack += def.attack;
                defence += def.defence;
                fire += def.fireRes;
            }
            Attack = Mathf.Max(1, attack);
            Defence = Mathf.Max(1, defence);
            FireRes = fire;
        }
    }
}
