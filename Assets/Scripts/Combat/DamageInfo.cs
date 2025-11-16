
using UnityEngine;
using System;

namespace InfimaGames.LowPolyShooterPack
{

    [System.Serializable]
    public struct DamageInfo
    {
        public float amount;
        public GameObject source;
        public Vector3 hitPoint;
        public Vector3 hitNormal;
        public bool isHeadshot;
        
        public DamageInfo(float damage, GameObject damageSource = null, 
            Vector3 point = default, Vector3 normal = default, bool headshot = false)
        {
            amount = damage;
            source = damageSource;
            hitPoint = point;
            hitNormal = normal;
            isHeadshot = headshot;
        }
    }

}