using UnityEngine;
using TMPro;

using UnityEngine.UI;
public class GrowMessageBubble : MonoBehaviour
{
    private TMP_Text text;
    private LayoutElement layout;
    // private RectTransform size;
    // private VerticalLayoutGroup verticalExpand;
    private ContentSizeFitter parentContent;
    [SerializeField] GameObject image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TMP_Text>();
        layout = GetComponent<LayoutElement>();
        // size = GetComponent<RectTransform>();
        // verticalExpand = image.GetComponent<VerticalLayoutGroup>();
        parentContent = image.GetComponent<ContentSizeFitter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (text.preferredWidth>3100){
                layout.preferredWidth = 3100f;
                
                parentContent.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                // verticalExpand.childForceExpandWidth=false;
                // verticalExpand.childForceExpandHeight=true;
        }
    }
}
