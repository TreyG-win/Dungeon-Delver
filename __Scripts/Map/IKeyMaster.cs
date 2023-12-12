using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IkeyMaster
{
    // This will allow you to get and set the number of keys
    int keyCount { get; set; }
    // Should already be in the "Dray()" class
    int GetFacing();
}
