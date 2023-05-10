using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Attributes;
using Util.Coroutine;
using Util.Enums;
using Util.Input;
using Util.Systems;

namespace Util.UI.Controllers
{
    public class CanvasController : MonoBehaviour
    {
        [Header("Input")] 
        [SerializeField, Interface(typeof(IMenuInputReader))]
        private ScriptableObject _menuInputReaderSO = default;
        private IMenuInputReader _menuInputReader;

        [Header("Configuration")]
        [SerializeField] protected UIPage _defaultUiPage;
        [SerializeField] protected bool _setGameStateOnStart = true;

        [Header("Data")]
        [SerializeField, ReadOnly] protected List<UIController> _uiControllers;
        [SerializeField, ReadOnly] protected Hashtable _uiHashtable;
        [SerializeField, ReadOnly] protected Stack<UIPage> _activeUiPages;

        public UIPage ActivePage => _activeUiPages.TryPeek(out var page) ? page : null;

        void Awake()
        {
            _menuInputReader = (IMenuInputReader)_menuInputReaderSO;

            _uiControllers = GetComponentsInChildren<UIController>().ToList();
            _uiHashtable = new Hashtable();
            _activeUiPages = new Stack<UIPage>(_uiControllers.Count);

            RegisterUIControllers(_uiControllers);
        }

        void OnEnable()
        {
            _menuInputReader.MenuCancelButtonEvent += ReturnToPrevious;
        }

        void OnDisable()
        {
            _menuInputReader.MenuCancelButtonEvent -= ReturnToPrevious;
        }

        void Start()
        {
            foreach (var controller in _uiControllers)
                controller.Disable();

            EnableUI(_defaultUiPage);

            if (_setGameStateOnStart == true)
                GameSystem.Instance.ChangeGameState(GameState.Menu);
        }

        /// <summary>
        /// Returns to the previously active UI page.
        /// </summary>
        public void ReturnToPrevious()
        {
            GetUI(ActivePage)?.ReturnToUI();
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">The UI page to be enabled.</param>
        /// <param name="resetOnSwitch"></param>
        public void EnableUI(UIPage target, bool resetOnSwitch = false)
        {
            if (target == null) return;

            _activeUiPages.Push(target);

            GetUI(target)?.Enable(resetOnSwitch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resetOnSwitch"></param>
        /// <param name="transition"></param>
        /// <returns></returns>
        public IEnumerator EnableUICoroutine(UIPage target, bool resetOnSwitch = false, bool transition = true)
        {
            if (target == null) yield break;

            _activeUiPages.Push(target);

            yield return GetUI(target)?.EnableCoroutine(resetOnSwitch, transition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">The UI page to be disabled.</param>
        public void DisableUI(UIPage target)
        {
            if (target == null) return;

            GetUI(target)?.Disable();
            
            _activeUiPages.Pop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resetOnSwitch"></param>
        /// <returns></returns>
        public IEnumerator DisableUICoroutine(UIPage target, bool resetOnSwitch = false)
        {
            if (target == null) yield break;

            yield return GetUI(target)?.DisableCoroutine();

            _activeUiPages.Pop();
        }

        /// <summary>
        /// Disables the currently active UI and enables the <c>target</c> UI page. 
        /// </summary>
        /// <param name="target">The desired UI page to display.</param>
        /// <param name="resetCurrentOnSwitch">Whether the current UI page should be reset.</param>
        /// <param name="resetTargetOnSwitch">Whether the target UI page should be reset.</param>
        /// <param name="transition">Whether tweens should be animated.</param>
        public void SwitchUI(UIPage target, bool resetCurrentOnSwitch = false, bool resetTargetOnSwitch = true, bool transition = true)
        {
            if (ActivePage == target || GetUI(ActivePage)?.IsTransitioning == true)
                return;

            StartCoroutine(CoroutineUtil.Sequence(
                DisableUICoroutine(ActivePage, resetCurrentOnSwitch),
                EnableUICoroutine(target, resetTargetOnSwitch, transition)
                ));
        }

        /// <summary>
        /// Disables the currently active UI and enables the <c>target</c> UI page. 
        /// </summary>
        /// <param name="target">The desired UI page to display.</param>
        /// <param name="resetCurrentOnSwitch">Whether the current UI page should be reset.</param>
        /// <param name="resetTargetOnSwitch">Whether the target UI page should be reset.</param>
        /// <param name="transition">Whether tweens should be animated.</param>
        public void DisplayUI(UIPage target, bool resetTargetOnSwitch = true, bool transition = true)
        {
            if (ActivePage == target || GetUI(ActivePage)?.IsTransitioning == true)
                return;

            GetUI(ActivePage)?.OnLoseFocus();

            StartCoroutine(EnableUICoroutine(target, resetTargetOnSwitch, transition));
        }

        /// <summary>
        /// Disables the currently active UI and enables the <c>target</c> UI page. 
        /// </summary>
        /// <param name="target">The desired UI page to display.</param>
        /// <param name="resetCurrentOnSwitch">Whether the current UI page should be reset.</param>
        public void HideUI(UIPage target, bool resetCurrentOnSwitch = false)
        {
            if (GetUI(target)?.IsTransitioning == true)
                return;

            StartCoroutine(
                CoroutineUtil.Sequence(
                    DisableUICoroutine(target, resetCurrentOnSwitch),
                    CoroutineUtil.CallAction(() => GetUI(ActivePage)?.SetActive()))
            );
        }

        public UIController GetUI(UIPage page) => page == null ? null : (UIController) _uiHashtable[page];

        protected void RegisterUIControllers(IEnumerable<UIController> controllers)
        {
            foreach (var controller in controllers)
            {
                if (DoesPageExist(controller.Page) == false)
                    _uiHashtable.Add(controller.Page, controller);
            }
        }

        protected bool DoesPageExist(UIPage page) => _uiHashtable.ContainsKey(page);
    }
}
