using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public string name;

    [Multiline()]
    public string description;

    private bool isPointerOver = false;
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        TooltipSystem.Show(description, name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        TooltipSystem.Hide();
    }

    /*
        public void OnMouseEnter()
        {
            TooltipSystem.Show(description, name);
        }

        public void OnMouseExit()
        {
            TooltipSystem.Hide();
        }
    */

    private void OnDisable()
    {
        TooltipSystem.Hide();
    }

    private void OnDestroy()
    {
        TooltipSystem.Hide();
    }

}
