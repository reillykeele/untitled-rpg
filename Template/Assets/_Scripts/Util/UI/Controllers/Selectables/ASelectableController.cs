using UnityEngine;
using UnityEngine.EventSystems;

namespace Util.UI.Controllers.Selectables
{
    [RequireComponent(typeof(UnityEngine.UI.Selectable))]
    public abstract class ASelectableController : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        protected CanvasController _canvasController;
        protected UIController _uiController;
        protected UnityEngine.UI.Selectable _selectable;

        protected virtual void Awake()
        {
            _canvasController = GetComponentInParent<CanvasController>();
            _uiController = GetComponentInParent<UIController>();
            _selectable = GetComponent<UnityEngine.UI.Selectable>();
        }

        protected virtual void OnEnable()
        {
            if (EventSystem.current?.currentSelectedGameObject == gameObject)
                Select();
        }

        protected virtual void OnDisable()
        {

        }

        public virtual void Select() => _selectable.Select();

        public virtual void OnSelect(BaseEventData eventData) { }
        public virtual void OnDeselect(BaseEventData eventData) { }
        public virtual void OnPointerEnter(PointerEventData eventData) => Select();
        public virtual void OnPointerExit(PointerEventData eventData) => EventSystem.current.SetSelectedGameObject(null);
    }
}