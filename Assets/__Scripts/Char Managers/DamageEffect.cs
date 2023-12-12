using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEffect : MonoBehaviour
{
    [Header("Set in Inspector")]
    //Modifies the amount of damage an attack does
    public int damage = 1;
    //Determines if knockback is enabled for an attack, by default it is
    public bool knockback = true;
}
