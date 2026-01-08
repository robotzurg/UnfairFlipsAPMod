using UnityEngine;
using UnityEngine.EventSystems;

namespace UnfairFlipsAPMod
{
    public class ShopButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string tooltipText;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(tooltipText))
            {
                TooltipHandler.Instance.Show(tooltipText, GetComponent<RectTransform>());
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipHandler.Instance.Hide();
        }

        private void OnDisable()
        {
            if (TooltipHandler.Instance != null)
                TooltipHandler.Instance.Hide();
        }
    }
}
