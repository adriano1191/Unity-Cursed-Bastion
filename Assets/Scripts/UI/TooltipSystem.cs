using Unity.VisualScripting;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{

    private static TooltipSystem current;
    public Tooltip tooltip;

    public void Awake()
    {
        current = this;
        Hide();

    }

    public static void Show(string description, string name = "")
    {
        current.tooltip.SetText(description, name);
        current.tooltip.gameObject.SetActive(true);
        
    }

    public static void Hide()
    {
        if (current != null)
        {
            current.tooltip.gameObject.SetActive(false);
        }


    }



}
