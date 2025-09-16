using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class BuffEffect : ScriptableObject
{
    public virtual void OnStart(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude) 
    { 

    }

    public virtual void OnEnd(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude) 
    {

    }
}
