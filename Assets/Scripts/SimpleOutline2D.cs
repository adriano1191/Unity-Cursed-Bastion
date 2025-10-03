using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class SimpleOutline2D : MonoBehaviour
{
    public Color outlineColor = Color.yellow;
    [Range(1, 100)] public int outlineSizePx = 2;

    SpriteRenderer src;
    readonly List<SpriteRenderer> rims = new List<SpriteRenderer>();
    readonly Vector2[] dirs = {
        new( 1, 0), new(-1, 0), new(0, 1), new(0,-1),
        new( 1, 1), new(-1, 1), new(1,-1), new(-1,-1)
    };

    void OnEnable()
    {
        src = GetComponent<SpriteRenderer>();
        EnsureChildren();
        SyncAll();
    }

    void OnDisable()
    {
        foreach (var r in rims) if (r) r.enabled = false;
    }

    void Update() { SyncAll(); }

    void EnsureChildren()
    {
        if (rims.Count == 8 && rims.TrueForAll(r => r)) return;
        rims.Clear();
        for (int i = 0; i < 8; i++)
        {
            var go = new GameObject($"{name}_Outline_{i}");
            go.transform.SetParent(transform, false);
            var r = go.AddComponent<SpriteRenderer>();
            rims.Add(r);
        }
    }

    void SyncAll()
    {
        if (!src) return;

        float ppu = src.sprite ? src.sprite.pixelsPerUnit : 100f;
        float step = outlineSizePx / ppu;

        for (int i = 0; i < rims.Count; i++)
        {
            var r = rims[i];
            if (!r) continue;

            // pozycja i skala
            r.transform.localPosition = (Vector3)(dirs[i] * step);
            r.transform.localRotation = Quaternion.identity;
            r.transform.localScale = Vector3.one;

            // wygl¹d
            r.sprite = src.sprite;
            r.color = outlineColor;

            // warstwy/sortowanie (pod spodem)
            r.sortingLayerID = src.sortingLayerID;
            r.sortingOrder = src.sortingOrder - 1;

            // pozosta³e w³aœciwoœci
            r.flipX = src.flipX;
            r.flipY = src.flipY;
            r.drawMode = src.drawMode;
            r.maskInteraction = src.maskInteraction;
            r.enabled = src.enabled;
            r.sharedMaterial = src.sharedMaterial; // Sprites/Default OK
        }
    }
}