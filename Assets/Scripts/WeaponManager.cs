using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    List<Weapon> weapons = new List<Weapon>();
    [SerializeField] Weapon nowWeapon = null;
    void Awake()
    {
        weapons.AddRange(GetComponentsInChildren<Weapon>());

        foreach (var w in weapons)
            w.gameObject.SetActive(false);

        SetWeapon(1);
    }

    public Weapon GetWeapon()
    {
        return nowWeapon;
    }

    public void SetWeapon(int index)
    {
        {
            // ���� ���� ����
            if (nowWeapon != null)
            {
                nowWeapon.gameObject.SetActive(false);
            }

            // ������ ���� ����
            if (index < weapons.Count)
            {
                nowWeapon = weapons[index];
            }
            else
            {
                nowWeapon = weapons[0];
            }
            nowWeapon.gameObject.SetActive(true);
        }
    }
}
