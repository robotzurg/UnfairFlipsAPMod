using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnfairFlipsAPMod
{
    public class TooltipHandler : MonoBehaviour
    {
        private static TooltipHandler _instance;
        public static TooltipHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("TooltipHandler");
                    _instance = go.AddComponent<TooltipHandler>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private GameObject tooltipObject;
        private TextMeshProUGUI tooltipText;
        private RectTransform tooltipRect;
        private Image background;
        private Canvas canvas;
        private RectTransform currentTarget;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            CreateTooltip();
        }

        private void CreateTooltip()
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            tooltipObject = new GameObject("ShopTooltip");
            tooltipObject.transform.SetParent(canvas.transform, false);
            tooltipRect = tooltipObject.AddComponent<RectTransform>();
            
            background = tooltipObject.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.8f);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(tooltipObject.transform, false);
            tooltipText = textGo.AddComponent<TextMeshProUGUI>();
            tooltipText.fontSize = 24;
            tooltipText.alignment = TextAlignmentOptions.Center;
            tooltipText.color = Color.white;
            
            var messageManager = FindObjectOfType<MessageManager>();
            if (messageManager != null && messageManager.prf_message != null)
            {
                var sampleText = messageManager.prf_message.GetComponent<TMP_Text>();
                if (sampleText != null) tooltipText.font = sampleText.font;
            }

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-15, -15);

            tooltipObject.SetActive(false);
        }

        public void Show(string text, RectTransform target = null)
        {
            if (tooltipObject == null) CreateTooltip();
            if (tooltipObject == null) return;

            currentTarget = target;
            tooltipText.text = text;
            tooltipObject.SetActive(true);
            tooltipObject.transform.SetAsLastSibling();
            
            // Adjust size to text first so UpdatePosition knows the size
            var size = tooltipText.GetPreferredValues(text);
            tooltipRect.sizeDelta = new Vector2(size.x + 30, size.y + 30);

            UpdatePosition();
        }

        public void Hide()
        {
            currentTarget = null;
            if (tooltipObject != null)
                tooltipObject.SetActive(false);
        }

        private void Update()
        {
            if (tooltipObject != null && tooltipObject.activeSelf)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            if (currentTarget != null)
            {
                // Position under the target button
                // Get the world corners of the target button
                Vector3[] corners = new Vector3[4];
                currentTarget.GetWorldCorners(corners);
                
                // corners[0] = bottom-left, [1] = top-left, [2] = top-right, [3] = bottom-right
                // We want to center it horizontally under the button
                float centerX = (corners[0].x + corners[3].x) / 2f;
                float bottomY = corners[0].y;

                tooltipRect.position = new Vector3(centerX, bottomY - 5, 0);
                tooltipRect.pivot = new Vector2(0.5f, 1f); // Top-center pivot
            }
            else
            {
                Vector2 mousePos = Input.mousePosition;
                // Offset from cursor
                tooltipRect.position = mousePos + new Vector2(20, 20);
                
                // Keep on screen
                var pivot = new Vector2(0, 0);
                if (mousePos.x + tooltipRect.sizeDelta.x > Screen.width) pivot.x = 1;
                if (mousePos.y + tooltipRect.sizeDelta.y > Screen.height) pivot.y = 1;
                tooltipRect.pivot = pivot;
            }
        }
    }
}
