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
    [SerializeField] float baseAttackSpeed = 1f;
    //public float attackSpeed = 1f; // attacks per second
    public float attackSpeedMultiplierPerAgility = 0.02f; // ka¿dy punkt zrêcznoœci zwiêksza prêdkoœæ ataku o 2%


    public int Strength => baseStrength;        // tylko do odczytu
    public event Action<int, int> StrengthChanged; // (oldStr, newStr)
    public int Agility => baseAgility;
    public event Action<int, int> AgilityChanged; // (oldAg, newAg)
    public int Intellect => baseIntellect;
    public int CurrentXP => currentXP;
    public event Action<int, int> XpChanged; // (oldXp, newXp)
    public float AttackSpeed => baseAttackSpeed;
    public event Action<float, float> attackSpeedChange; 


    public enum StatType
    {
        Strength,
        Agility,
        Intellect,
        Experience,
        AttackSpeed
    }

    private void OnEnable()
    {
        AgilityChanged += OnAgilityChanged; // subskrybuj zdarzenie zmiany zrêcznoœci
        OnAgilityChanged(0, Agility); // inicjalne ustawienie speed na podstawie aktualnej zrêcznoœci
        attackSpeedChange += OnAttackSpeedChanged;
        OnAttackSpeedChanged(0, AttackSpeed);

        //XpChanged += OnExperienceChanged;
        //OnExperienceChanged(0, CurrentXP);
    }

    public void AddStats(StatType stat, int value)
    {
        if (value == 0) return;
        switch (stat)
        {
            case StatType.Strength:
                int oldStr = baseStrength;
                baseStrength += value;
                StrengthChanged?.Invoke(oldStr, baseStrength);
                Debug.Log($"PlayerStats: Strength changed from {oldStr} to {baseStrength}");
                break;
            case StatType.Agility:
                int oldAg = baseAgility;
                baseAgility += value;
                AgilityChanged?.Invoke(oldAg, baseAgility);
                Debug.Log($"PlayerStats: Agility changed from {oldAg} to {baseAgility}");
                break;
            case StatType.Intellect:
                baseIntellect += value;
                break;
            case StatType.Experience:
                int oldExp = currentXP;
                currentXP += value;
                XpChanged?.Invoke(oldExp, currentXP);
                Debug.Log($"PlayerStats: Xp changed from {oldExp} to {currentXP}");
                break;
            case StatType.AttackSpeed:
                float oldAttackSpeed = baseAttackSpeed;
                baseAttackSpeed -= value;
                attackSpeedChange?.Invoke(oldAttackSpeed, baseAttackSpeed);
                Debug.Log($"PlayerStats: Attack Speed changed from {oldAttackSpeed} to {baseAttackSpeed}");
                break;
            default:
                Debug.LogWarning("Nieznany typ statystyki!");
                break;
        }
    }

    public void SetStats(StatType stat, int value)
    {
        if (value < 0) value = 0;
        switch (stat)
        {
            case StatType.Strength:
                if (value == baseStrength) return;
                int oldStr = baseStrength;
                baseStrength = value;
                StrengthChanged?.Invoke(oldStr, baseStrength);
                Debug.Log($"PlayerStats: Strength set from {oldStr} to {baseStrength}");
                break;
            case StatType.Agility:
                if (value == baseAgility) return;
                int oldAg = baseAgility;
                baseAgility = value;
                AgilityChanged?.Invoke(oldAg, baseAgility);
                Debug.Log($"PlayerStats: Agility set from {oldAg} to {baseAgility}");
                break;
            case StatType.Intellect:
                if (value == baseIntellect) return;
                baseIntellect = value;
                break;
            case StatType.Experience:
                if (value == currentXP) return;
                int oldExp = currentXP;
                currentXP = value;
                AgilityChanged?.Invoke(oldExp, currentXP);
                Debug.Log($"PlayerStats: XP set from {oldExp} to {currentXP}");
                break;
            case StatType.AttackSpeed:
                if (value == baseAttackSpeed) return;
                float oldAttackSpeed = baseAttackSpeed;
                baseAttackSpeed = value;
                attackSpeedChange?.Invoke(oldAttackSpeed, baseAttackSpeed);
                Debug.Log($"PlayerStats: Attack Speed set from {oldAttackSpeed} to {baseAttackSpeed}");
                break;
            default:
                Debug.LogWarning("Nieznany typ statystyki!");
                break;
        }
    }



    public void OnAgilityChanged(int oldAgility, int newAgility)
    {
        // Za³ó¿my, ¿e ka¿da jednostka zrêcznoœci zwiêksza prêdkoœæ o 0.5f
        //attackSpeed = attackSpeed - (baseAttackSpeed * attackSpeedMultiplierPerAgility);
        float oldAttackSpeed = AttackSpeed;
        float newAttackSpeed = oldAttackSpeed - (newAgility * attackSpeedMultiplierPerAgility);
        //Debug.Log($"Calculating new Attack Speed: {oldAttackSpeed} - ({newAgility} * {attackSpeedMultiplierPerAgility}) = {newAttackSpeed}");
        attackSpeedChange?.Invoke(oldAttackSpeed, newAttackSpeed);
        //Debug.Log($"Agility changed from {oldAgility} to {newAgility}, Attack Speed updated from {oldAttackSpeed} to {newAttackSpeed}");

    }

    public void OnAttackSpeedChanged(float oldAttackSpeed, float newAttackSpeed)
    {

        // Tutaj mo¿esz dodaæ logikê, która ma siê wykonaæ po zmianie prêdkoœci ataku

        //Debug.Log($"Attack Speed changed from {oldAttackSpeed} to {newAttackSpeed}");
    }


    // Przyk³ady metod do modyfikacji si³y (Strength)
    /*
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
*/


}






