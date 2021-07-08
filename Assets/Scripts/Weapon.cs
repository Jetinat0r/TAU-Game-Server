using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon
{
    public string name;
    public float damage;
    public float velocity;
    //1 plus or minus this multiplier
    [Range(0, 1)]
    public float velocityRandomizer;

    public int numShots;
    [Range(0, 90)]
    public float shotAngle;
    public float shotOffset;

    public GameObject bullet;
}
