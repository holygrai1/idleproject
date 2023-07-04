using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public RectTransform parent;
    public Text tex;
    public RectTransform texRect;
    public bool check = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (check == true)
        {
            Debug.Log("preferredWidth: " + tex.preferredWidth);
            Debug.Log("preferredHeight: " + tex.preferredHeight);
            Debug.Log("flexibleWidth: " + tex.flexibleWidth);

            //parent.sizeDelta = new Vector2(tex.preferredWidth + 10, parent.sizeDelta.y);
            //if (tex.preferredWidth > 300)
            //{
            //    var widthPerLine = tex.preferredWidth / 3.0f + 10.0f;
            //    var height = tex.preferredHeight ;
            //    texRect.sizeDelta = new Vector2(widthPerLine, height);
            //    parent.sizeDelta = new Vector2(widthPerLine, height);
            //}
            //else if (tex.preferredWidth > 100)
            //{
            //    var widthPerLine = tex.preferredWidth / 2.0f + 10.0f;
            //    var height = tex.preferredHeight  ;
            //    texRect.sizeDelta = new Vector2(widthPerLine, height);
            //    parent.sizeDelta = new Vector2(widthPerLine, height);
            //}
            //else
            //{
            //    texRect.sizeDelta = new Vector2(tex.preferredWidth + 10, tex.preferredHeight);
            //    parent.sizeDelta = new Vector2(tex.preferredWidth + 10, tex.preferredHeight);
            //}

            float maxWidth = 200.0f;
            if (tex.preferredWidth > 200)
            {
                int lineNum = Mathf.CeilToInt(tex.preferredWidth / maxWidth);
                lineNum = Mathf.Min(3, lineNum);
                var height = 16 * lineNum;
                texRect.sizeDelta = new Vector2(maxWidth, height);
                parent.sizeDelta = new Vector2(maxWidth, height);
            }
            else
            {
                texRect.sizeDelta = new Vector2(tex.preferredWidth + 10, 16);
                parent.sizeDelta = new Vector2(tex.preferredWidth + 10, 16);
            }
        }
    }
}
