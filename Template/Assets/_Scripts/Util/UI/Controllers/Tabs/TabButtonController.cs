using UnityEngine;
using UnityEngine.UI;
using Util.Helpers;
using Util.UI.Controllers.Selectables.Buttons;

namespace Util.UI.Controllers.Tabs
{
    [RequireComponent(typeof(Button))]
    public class TabButtonController : AButtonController
    {
        [SerializeField] private GameObject _tabContent;

        private int _tabIndex = 0;
        private TabUIController _tabController;

        public void SetTabIndex(TabUIController tabController, int tabIndex)
        {
            _tabController = tabController;
            _tabIndex = tabIndex;
        }

        protected override void OnClick()
        {
            _tabController.SwitchTab(_tabIndex);
        }

        public void Show()
        {
            _tabContent.Enable();
        }

        public void Hide()
        {
            _tabContent.Disable();
        }

    }
}
