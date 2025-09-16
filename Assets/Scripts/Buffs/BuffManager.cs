using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class ActiveBuff
{
    public BuffEffect effect;
    public float timeLeft, duration;
    public float magnitude;
    public int stacks;  
    public ActiveBuff(BuffEffect effect, float duration, float magnitude, int stacks)
    {
        this.effect = effect;
        this.duration = duration;
        this.timeLeft = duration;
        this.magnitude = magnitude;
        this.stacks = stacks;
    }
}

public class BuffManager : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] List<ActiveBuff> activeBuffsList = new();
    [SerializeField] int maxStacks = 99;

    private void Awake()
    {
        if(playerStats == null)
            playerStats = GetComponent<PlayerStats>();
            playerHealth = GetComponent<PlayerHealth>();
    }

    public void AddBuff(BuffEffect buffEffect, float duration, float magnitude, int stacks = 1, bool refresh=true, bool stackable=false)
    {

        Debug.Log($"-1. BuffManager: Attempting to add buff {buffEffect.name}, Stacks: {stacks}, Duration: {duration}, Magnitude: {magnitude}");
        var buff = activeBuffsList.Find(x => x.effect == buffEffect);
        Debug.Log($"var buff = {buff}");
        Debug.Log($"0. BuffManager: Found existing buff: {(buff != null ? "Yes" : "No")}");
        if (buff != null)
        {
            Debug.Log($"1. BuffManager: Buff {buffEffect.name} already active. Current Stacks: {buff.stacks}, Duration Left: {buff.timeLeft}, Magnitude: {buff.magnitude}");
            if (refresh)
            {
                buff.duration = duration;
                buff.timeLeft = duration;
                Debug.Log($"2. BuffManager: Refreshed buff {buffEffect.name}, Duration: {duration}");
            }
            Debug.Log($"2.5 Stacks: {stacks}, {maxStacks} - {buff.stacks}");
            if(!stackable) return;
            int add = Mathf.Min(stacks, maxStacks - buff.stacks);
            Debug.Log($"3. BuffManager: Adding {add} stacks to buff {buffEffect.name}. New Stacks: {buff.stacks + add}");
            if (add > 0)
            {
                buff.stacks += add;
                buffEffect.OnStart(playerStats, playerHealth, add, magnitude);
                Debug.Log($"4. BuffManager: Buff {buffEffect.name} stacks increased to {buff.stacks}");
                return;

            }
            ActiveBuff activeBuff = new ActiveBuff(buffEffect, duration, magnitude, stacks);
            activeBuffsList.Add(activeBuff);
            buffEffect.OnStart(playerStats, playerHealth, stacks, magnitude);
            Debug.Log($"5. BuffManager: Added new buff {buffEffect.name}, Stacks: {stacks}, Duration: {duration}, Magnitude: {magnitude}");
        }
        ActiveBuff newBuff = new(buffEffect, duration, magnitude, stacks);
        activeBuffsList.Add(newBuff);
        buffEffect.OnStart(playerStats, playerHealth, stacks, magnitude);

        Debug.Log($"6. BuffManager: Added new buff {buffEffect.name}, Stacks: {stacks}, Duration: {duration}, Magnitude: {magnitude}");

        /*
        if (buffEffect == null || duration <= 0 || stacks <= 0) return;
        ActiveBuff existingBuff = activeBuffs.Find(b => b.effect == buffEffect);
        if (existingBuff != null)
        {
            existingBuff.stacks = Mathf.Min(existingBuff.stacks + stacks, maxStacks);
            existingBuff.timeLeft = duration; // reset duration
            existingBuff.magnitude = magnitude; // update magnitude
            Debug.Log($"BuffManager: Refreshed buff {buffEffect.name}, Stacks: {existingBuff.stacks}, Duration: {duration}, Magnitude: {magnitude}");
        }
        else
        {
            ActiveBuff newBuff = new(buffEffect, duration, magnitude, stacks);
            activeBuffs.Add(newBuff);
            buffEffect.OnStart(playerStats, stacks, magnitude);
            Debug.Log($"BuffManager: Added new buff {buffEffect.name}, Stacks: {stacks}, Duration: {duration}, Magnitude: {magnitude}");
        }
        */
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = activeBuffsList.Count - 1; i >= 0; i--)
        {
            ActiveBuff buff = activeBuffsList[i];
            buff.timeLeft -= deltaTime;
            if (buff.timeLeft <= 0)
            {
                buff.effect.OnEnd(playerStats, playerHealth, buff.stacks, buff.magnitude);
                activeBuffsList.RemoveAt(i);
                Debug.Log($"BuffManager: Buff {buff.effect.name} ended.");
            }
        }
    }

    public void ClearAllBuffs()
    {
        foreach (var buff in activeBuffsList)
        {
            buff.effect.OnEnd(playerStats, playerHealth, buff.stacks, buff.magnitude);
        }
        activeBuffsList.Clear();
        Debug.Log("BuffManager: All buffs cleared.");
    }

}
