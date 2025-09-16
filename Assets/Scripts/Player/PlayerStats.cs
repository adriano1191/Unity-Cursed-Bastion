using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] int level = 1;
    [SerializeField] int currentXP = 0;
    [SerializeField] int[] xpToNextLevel = new int[] { 100, 200, 400, 800, 1600 };

    //[Header("Health")]
    //public int maxHealth = 100;
    //public int currentHealth;

    [Header("Base Stats")]
    [SerializeField] int baseStrength = 5;
    [SerializeField] int baseAgility = 5;
    [SerializeField] int baseIntellect = 5;
    [SerializeField] int baseEndurance = 5;
    [SerializeField] int baseLuck = 5;

    [Header("Secondary Stats")]
    public float moveSpeed = 5f;
    public int attackPower = 20;


    public int Strength => baseStrength;        // tylko do odczytu
    public event Action<int, int> StrengthChanged; // (oldStr, newStr)

    public void AddStrength(int amount)
    {
        if (amount == 0) return;
        int oldStr = baseStrength;
        baseStrength += amount;
        StrengthChanged?.Invoke(oldStr, baseStrength);
        Debug.Log($"PlayerStats: Strength changed from {oldStr} to {baseStrength}");
    }

    public void SetStrength( int value)
    {
        if (value < 0) value = 0;
        if (value == baseStrength) return;
        int old = baseStrength;
        baseStrength = value;
        StrengthChanged?.Invoke(old, baseStrength);
        Debug.Log($"PlayerStats: Strength set from {old} to {baseStrength}");
    }

}






