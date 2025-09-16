using UnityEngine;

public class MonsterStats : MonoBehaviour
{
    [Header("Konfiguracja")]
    public int maxHealth = 100;
    public int attackPower = 20;
    public float attackRange = 0.5f;
    public float attackCooldown = 1.0f;
    public float moveSpeed = 2.5f;
    public bool rangedAttack = false;
    public bool meleeAttack = true;

}
