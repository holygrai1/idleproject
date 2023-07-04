using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ??????????
/// </summary>
public static class Extensions
{
    public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    public static bool IsNull(this UnityEngine.Object o)
    {
        return o == null;
    }

    public static TMPro.TextMeshProUGUI[] GetComponentsInChildrenTextMeshProUGUI(this GameObject go)
    {
        return go.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
    }

    public static TMPro.TextMeshPro[] GetComponentsInChildrenTextMeshPro(this GameObject go)
    {
        return go.GetComponentsInChildren<TMPro.TextMeshPro>(true);
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
        }

        return comp;
    }

    public static void SetTextWithEllipsis(this Text textComponent, string value)
    {
        var generator = textComponent.cachedTextGenerator;
        var rectTransform = textComponent.GetComponent<RectTransform>();
        var settings = textComponent.GetGenerationSettings(rectTransform.rect.size);
        generator.Populate(value, settings);
        var characterCountVisible = generator.characterCountVisible;
        var updatedText = value;
        if (value.Length > characterCountVisible)
        {
            updatedText = value.Substring(0, characterCountVisible - 1);
            updatedText += "...";
        }
        textComponent.text = updatedText;
    }
}
