using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Util.UI.Controllers.Tabs
{
    public class TabUIController : UIController
    {
        [SerializeField] private int _initialTab = 0;

        private List<TabButtonController> _tabs;

        private int _activeTabIndex = -1;

        protected override void Awake()
        {
            base.Awake();

            _tabs = GetComponentsInChildren<TabButtonController>().ToList();
        }

        void Start()
        {
            for (var i = 0; i < _tabs.Count; i++)
            {
                _tabs[i].SetTabIndex(this, i);

                if (i != _initialTab)
                    _tabs[i].Hide();
            }
        }

        public void SwitchTab(int tabIndex)
        {
            if (_activeTabIndex >= 0)
                _tabs[_activeTabIndex].Hide();

            _activeTabIndex = tabIndex;
            _tabs[_activeTabIndex].Show();
        }

    }
}
