using System.Collections.Generic;
using TMPro; // jeœli u¿ywasz TextMeshPro
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuffsUi : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private BuffManager buffManager; // przypisz z inspektora
    [SerializeField] private GameObject panel; // panel który siê pokazuje
    [SerializeField] private Transform contentParent; // parent z GridLayout
    [SerializeField] private GameObject itemSlotPrefab;

    [SerializeField] private TooltipTrigger tooltipTrigger; // przypisz z inspektora




    private List<GameObject> spawnedSlots = new();

    [SerializeField] private InputAction toggleInventoryAction;

    void OnEnable() => toggleInventoryAction.Enable();
    void OnDisable() => toggleInventoryAction.Disable();

    void Start()
    {
        //panel.SetActive(false);
        if (!buffManager) buffManager = player.GetComponent<BuffManager>();
        if (!tooltipTrigger) tooltipTrigger = FindFirstObjectByType<TooltipTrigger>();

    }

    void Update()
    {
        /*
        if (toggleInventoryAction.WasPressedThisFrame())
        {
            bool activePanel = !panel.activeSelf;
            panel.SetActive(activePanel);
            //panelWeapon.SetActive(activeWeaponPanel);
            if (activePanel) RefreshUI();
        }
        */

        RefreshUI();
    }

    private void RefreshUI()
    {
        var slots = buffManager.GetActiveBuffs();

        // Rozszerz pulê jeœli potrzeba
        while (spawnedSlots.Count < slots.Count)
        {
            var go = Instantiate(itemSlotPrefab, contentParent);
            spawnedSlots.Add(go);
        }

        // Aktualizuj widoczne sloty
        for (int i = 0; i < slots.Count; i++)
        {
            var stack = slots[i];
            var go = spawnedSlots[i];
            go.SetActive(true);

            var icon = go.transform.Find("Icon").GetComponent<Image>();
            var clockFrame = go.transform.Find("ClockFrame").GetComponent<Image>();
            var countText = go.transform.Find("Text").GetComponent<TMP_Text>();
            var tt = go.GetComponent<TooltipTrigger>();

            icon.sprite = stack.icon;
            clockFrame.fillAmount = 1f - (stack.timeLeft / stack.duration);
            countText.text = stack.timeLeft.ToString("F1");
            tt.name = stack.name;
            tt.description = stack.description;
        }

        // Ukryj nieu¿ywane sloty
        for (int i = slots.Count; i < spawnedSlots.Count; i++)
        {
            spawnedSlots[i].SetActive(false);
        }
    }

}
