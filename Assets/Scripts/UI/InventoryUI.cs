using System.Collections.Generic;
using TMPro; // jeœli u¿ywasz TextMeshPro
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private GameObject leftHand;
    private GameObject rightHand;
    [SerializeField] private Inventory inventory; // przypisz z inspektora
    [SerializeField] private GameObject panel; // panel który siê pokazuje
    [SerializeField] private GameObject panelWeapon; // weapon panel który siê pokazuje
    [SerializeField] private GameObject weaponLeft;
    [SerializeField] private GameObject weaponRight;
    [SerializeField] private Transform contentParent; // parent z GridLayout
    [SerializeField] private GameObject itemSlotPrefab;

    [SerializeField] private TooltipTrigger tooltipTrigger; // przypisz z inspektora




    private List<GameObject> spawnedSlots = new();

    [SerializeField] private InputAction toggleInventoryAction;

    void OnEnable() => toggleInventoryAction.Enable();
    void OnDisable() => toggleInventoryAction.Disable();

    void Start()
    {
        panel.SetActive(false);
        panelWeapon.SetActive(false);
        leftHand = player.transform.Find("LeftHand").gameObject;
        rightHand = player.transform.Find("RightHand").gameObject;
        if (!inventory) inventory = player.GetComponent<Inventory>();
        if (!tooltipTrigger) tooltipTrigger = FindFirstObjectByType<TooltipTrigger>();

    }

    void Update()
    {
        if (toggleInventoryAction.WasPressedThisFrame())
        {
            bool activePanel = !panel.activeSelf;
            bool activeWeaponPanel = !panelWeapon.activeSelf;
            panel.SetActive(activePanel);
            //panelWeapon.SetActive(activeWeaponPanel);
            if (activePanel) RefreshUI();
        }
    }

    void RefreshUI()
    {
        // usuñ stare ikony
        foreach (var s in spawnedSlots)
            Destroy(s);
        spawnedSlots.Clear();

        // wygeneruj nowe
        var slots = inventory.GetSlots(); // dodaj getter w Inventory!
        foreach (var stack in slots)
        {
            var go = Instantiate(itemSlotPrefab, contentParent);
            var icon = go.transform.Find("Icon").GetComponent<Image>();
            var countText = go.transform.Find("Text").GetComponent<TMP_Text>();
            var tt = go.GetComponent<TooltipTrigger>();

            icon.sprite = stack.def.icon; // zak³adam, ¿e ItemDefinition ma pole `public Sprite icon`
            countText.text = stack.count > 1 ? stack.count.ToString() : "";
            tt.name = stack.def.displayName;
            tt.description = stack.def.description;


            spawnedSlots.Add(go);
        }

        var iconWeaponLeft = weaponLeft.transform.Find("Icon").GetComponent<Image>();
        var iconWeaponRight = weaponRight.transform.Find("Icon").GetComponent<Image>();
        var sr = leftHand.GetComponentInChildren<SpriteRenderer>();

        //iconWeaponLeft.sprite = player.transform.Find("LeftHand").GetComponentInChildren<SpriteRenderer>().sprite;
        //iconWeaponLeft.sprite = leftHand.transform.Find("GFX").GetComponentInChildren<SpriteRenderer>().sprite;
       // iconWeaponRight.sprite = rightHand.transform.Find("GFX").GetComponentInChildren<SpriteRenderer>().sprite;




    }
}
