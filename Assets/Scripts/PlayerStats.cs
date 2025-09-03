using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerStats : EntityStats
    {
        [SerializeField] public float dashSpeed = 20f; //대쉬 속도
        [SerializeField] public float dashTime = 1f; //대쉬 시간
        [SerializeField] public float dashCoolTime = 2f; //대쉬 쿨타임
        //사망 처리
        protected override void Die ()
        {
            Debug.Log($"{gameObject.name} 사망 처리 필요");
            //Destroy(gameObject);
        }
    }
}