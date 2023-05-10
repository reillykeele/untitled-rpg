using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Util.Coroutine;
using Util.Enums;
using Util.GameEvents;
using Util.Helpers;
using Util.Singleton;
using Util.UI.Controllers;

namespace Util.Systems
{
    /// <summary>
    /// Handles smoothly loading and transitioning between scenes.
    /// </summary>
    public class LoadingSystem : Singleton<LoadingSystem>
    {
        [Header("Loading Events")]
        [SerializeField] private VoidGameEventSO _quitGameEvent = default;
        [SerializeField] private StringGameEventSO _changeSceneEvent = default;

        public UnityEvent OnSceneLoadedEvent = new UnityEvent();
        public UnityEvent OnLoadStartEvent = new UnityEvent();
        public UnityEvent OnLoadEndEvent = new UnityEvent();

        [Header("Configuration")]
        [SerializeField, Min(0f)] private float _minLoadingScreenTime = 0f;
        [SerializeField, Min(0f)] private float _fadeInDuration = 0.5f;
        [SerializeField, Min(0f)] private float _fadeOutDuration = 0.5f;

        public bool IsLoading { get; private set; }

        // Components
        private LoadingCanvasController _loadingCanvas;
        private UIController _uiController;
        private CanvasGroup _loadingCanvasGroup;

        protected override void Awake()
        {
            base.Awake();

            _loadingCanvas = GetComponentInChildren<LoadingCanvasController>();
            
            _uiController = _loadingCanvas.GetComponentInChildren<UIController>();
            _loadingCanvasGroup = _loadingCanvas.GetComponentInChildren<CanvasGroup>();
            
            _uiController.Disable();
        }

        void OnEnable()
        {
            _quitGameEvent.OnEventRaised += QuitGame;
            _changeSceneEvent.OnEventRaised += LoadScene;
        }

        void OnDisable()
        {
            _quitGameEvent.OnEventRaised -= QuitGame;
            _changeSceneEvent.OnEventRaised += LoadScene;
        }

        void Start()
        {
            OnSceneLoadedEvent.Invoke();
        }

        public void QuitGame()
        {
            StartCoroutine(CoroutineUtil.Sequence(
                UIHelper.FadeInAndEnable(_uiController, _loadingCanvasGroup, _fadeInDuration),
                CoroutineUtil.CallAction(() => GameSystem.Instance.Quit()))
            );
        }

        /// <summary>
        /// Unloads the active scene and loads the <c>toScene</c> scene, setting it to active.
        /// </summary>
        /// <param name="toSceneName)">The scene to load in.</param>
        public void TransitionScene(string toSceneName)
        {
            // Unload the active scene
            var unloadOp = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            // Load the next scene
            unloadOp.completed += (op) =>
            {
                var loadOp = SceneManager.LoadSceneAsync(toSceneName, LoadSceneMode.Additive);
                loadOp.completed += (o2) => SceneManager.SetActiveScene(SceneManager.GetSceneByName(toSceneName));
            };
        }

        /// <summary>
        ///  Unloads the <c>fromScene</c> scene and loads the <c>toScene</c> scene, setting it to active.
        /// </summary>
        /// <param name="fromScene">The scene to unload.</param>
        /// <param name="toScene">The scene to load in.</param>
        public void TransitionScene(string fromSceneName, string toSceneName)
        {
            // Unload the current level
            var unloadOp = SceneManager.UnloadSceneAsync(fromSceneName);

            // Load the next level
            unloadOp.completed += (op) =>
            {
                var loadOp = SceneManager.LoadSceneAsync(toSceneName, LoadSceneMode.Additive);
                loadOp.completed += (o2) => SceneManager.SetActiveScene(SceneManager.GetSceneByName(toSceneName));
            };
        }

        /// <summary>
        /// Unloads all currently loaded scenes to load in the specified scene. Displays a loading screen while
        /// loading in the new scene.
        /// </summary>
        /// <param name="scene">The scene to load in.</param>
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        /// <summary>
        /// Unloads all currently loaded scenes to load in the specified scene. Displays a loading screen while
        /// loading in the new scene.
        /// </summary>
        /// <param name="scene">The scene to load in.</param>
        /// <returns></returns>
        public IEnumerator LoadSceneCoroutine(string sceneName, bool manuallyEndLoading = false)
        {
            return CoroutineUtil.Sequence(
                SetLoading(true),
                LoadingScreen(sceneName),
                SetLoading(manuallyEndLoading),
                CoroutineUtil.CallAction(() =>
                {
                    Debug.Log("ON SCENE LOADED");
                    OnSceneLoadedEvent.Invoke();
                }));
        }

        public IEnumerator SetLoading(bool value)
        {
            if (value == true && !IsLoading)
            {
                GameSystem.Instance.ChangeGameState(GameState.Loading);
                IsLoading = true;
                OnLoadStartEvent.Invoke();
                yield return UIHelper.FadeInAndEnable(_uiController, _loadingCanvasGroup, _fadeInDuration);
            }
            else if (value == false)
            {
                IsLoading = false;
                OnLoadEndEvent.Invoke();
                yield return UIHelper.FadeOutAndDisable(_uiController, _loadingCanvasGroup, _fadeOutDuration);
            }
        }

        private IEnumerator LoadingScreen(string sceneName)
        {
            Time.timeScale = 1;

            var minEndTime = Time.time + _minLoadingScreenTime;
            var result = SceneManager.LoadSceneAsync(sceneName);
            result.completed += (op) => SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            // wait
            while (result.isDone == false || Time.time <= minEndTime)
            {
                yield return null;
            }
        }

    }
}
