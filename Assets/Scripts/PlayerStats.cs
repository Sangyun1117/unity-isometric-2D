using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerStats : EntityStats
    {

        //사망 처리
        protected override void Die ()
        {
            Debug.Log($"{gameObject.name} 사망 처리 필요");
            //Destroy(gameObject);
        }
    }
}