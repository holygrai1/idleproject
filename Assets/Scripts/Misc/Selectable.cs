using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Selectable : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    // Ts调用的action
    public Action<PointerEventData> SOnPointerClick;
    public Action<PointerEventData> SOnPointerDown;
    public Action<PointerEventData> SOnPointerEnter;
    public Action<PointerEventData> SOnPointerExit;
    public Action<PointerEventData> SOnPointerUp;

    public void OnPointerClick(PointerEventData eventData)
    {
        SOnPointerClick?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SOnPointerDown?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SOnPointerEnter?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SOnPointerExit?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SOnPointerUp?.Invoke(eventData);
    }
}
