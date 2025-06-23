using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class SpecialItem : Item
    {
        override protected void Init()
        {
            base.Init();
            _itemPart = ItemPart.Leg;
        }
    }
}

