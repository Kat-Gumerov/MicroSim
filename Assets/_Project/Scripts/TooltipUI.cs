using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [SerializeField] private TextMeshProUGUI text;

    void Awake()
    {
        Instance = this;
        Show(null);   // start hidden
    }

    public void Show(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            text.text = content;
        }
    }
}
