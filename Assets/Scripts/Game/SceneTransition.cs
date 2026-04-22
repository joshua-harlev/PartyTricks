using System;
  using System.Collections;
  using DG.Tweening;
  using UnityEngine;
  using UnityEngine.SceneManagement;
  using UnityEngine.UI;

  namespace Game {
      public class SceneTransition : MonoBehaviour {
          private static SceneTransition instance;

          [SerializeField] private float transitionDuration = 0.4f;

          private RectTransform panel;
          private Canvas canvas;
          private bool isTransitioning;

          private void Awake() {
              if (instance != null && instance != this) {
                  Destroy(gameObject);
                  return;
              }

              instance = this;
              DontDestroyOnLoad(gameObject);
              CreateTransitionUI();
          }

          private void CreateTransitionUI() {
              canvas = gameObject.AddComponent<Canvas>();
              canvas.renderMode = RenderMode.ScreenSpaceOverlay;
              canvas.sortingOrder = 999;

              gameObject.AddComponent<CanvasScaler>();

              var panelObject = new GameObject("TransitionPanel");
              panelObject.transform.SetParent(canvas.transform, false);

              var image = panelObject.AddComponent<Image>();
              image.color = Color.black;

              panel = panelObject.GetComponent<RectTransform>();
              panel.anchorMin = Vector2.zero;
              panel.anchorMax = Vector2.one;
              panel.offsetMin = Vector2.zero;
              panel.offsetMax = Vector2.zero;

              panel.anchoredPosition = new Vector2(-Screen.width, 0);
          }

          public static void LoadScene(string sceneName) {
              if (instance == null || instance.isTransitioning) {
                  SceneManager.LoadScene(sceneName);
                  return;
              }

              instance.StartCoroutine(instance.TransitionCoroutine(sceneName));
          }

          private IEnumerator TransitionCoroutine(string sceneName) {
              isTransitioning = true;

              panel.anchoredPosition = new Vector2(-Screen.width, 0);
              yield return panel.DOAnchorPosX(0, transitionDuration)
                  .SetEase(Ease.InQuad)
                  .SetUpdate(true)
                  .WaitForCompletion();

              SceneManager.LoadScene(sceneName);

              yield return null;

              yield return panel.DOAnchorPosX(Screen.width, transitionDuration)
                  .SetEase(Ease.OutQuad)
                  .SetUpdate(true)
                  .WaitForCompletion();

              panel.anchoredPosition = new Vector2(-Screen.width, 0);
              isTransitioning = false;
          }
      }
  }
