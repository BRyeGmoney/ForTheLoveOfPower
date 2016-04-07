using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.ForTheLoveOfPower.Scripts.PlayerControlled
{
    interface IObjectPoolItem
    {
        Boolean PoolObjInUse { get; set; }
    }
}
