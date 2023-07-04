using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Ts调用的action
    public Action<PointerEventData> SOnBeginDrag;
    public Action<PointerEventData> SOnDrag;
    public Action<PointerEventData> SOnEndDrag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        SOnBeginDrag?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        SOnDrag?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SOnEndDrag?.Invoke(eventData);
    }
}
