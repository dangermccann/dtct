using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


namespace DCTC.Controllers {
    public static class States {
        public static readonly string Title = "/title";
        public static readonly string GameMenu = "/game-menu";
        public static readonly string Map = "/map";
        public static readonly string NewGame = "/new-game";
        public static readonly string Loading = "/loading";
        public static readonly string Workforce = "/workforce";
        public static readonly string Finance = "/finance";
    }

    public class UrlProcessor {
        private static char[] delimiters = { '/', '\\' };

        public static UIStateInvocation MatchState(UIState[] states, string path) {

            UIStateInvocation invocation = new UIStateInvocation();
            UIState matchedState = null;
            string[] pathComponents = path.Split(delimiters);

            foreach(UIState state in states) {

                string[] patternComponents = state.UrlPattern.Split(delimiters);

                if (pathComponents.Length > patternComponents.Length)
                    continue;

                bool matched = true;
                for(int i = 0; i < patternComponents.Length; i++) {
                    // templates
                    if (patternComponents[i].StartsWith("{#") && patternComponents[i].EndsWith("}")) {
                        string variableName = patternComponents[i].Substring(2, patternComponents[i].Length - 3);
                        if (pathComponents.Length > i) {
                            invocation.Parameters.Add(variableName, Uri.UnescapeDataString(pathComponents[i]));
                        }
                    } else {
                        if (pathComponents.Length <= i || pathComponents[i] != patternComponents[i]) {
                            matched = false;
                            break;
                        }
                    }
                }

                if (matched) {
                    matchedState = state;
                    break;
                }
            }

            if (matchedState == null)
                return null;
            
            invocation.State = matchedState;
            return invocation;
        }
    }

    [System.Serializable]
    public class UIStateInvocation {
        public string Url;
        public UIState State;
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();

        public UIStateInvocation() { }
        public UIStateInvocation(UIState state, Dictionary<string, string> parameters) {
            this.State = state;
            this.Parameters = parameters;
        }
    }

	[System.Serializable]
	public class UIState {
		public string Name;
        public string UrlPattern;
		public bool Modal;
		public GameObject[] VisibleObjects;
		public UnityEvent OnBeforeExit;
        public UnityEvent OnEnter;
    }

	public class StateController : MonoBehaviour {
		public static StateController Get() {
            return GameObject.FindGameObjectWithTag("GameController").GetComponent<StateController>();
        }

		public UIState[] states;
		public GameObject uiRoot;
		public int maxStackDepth = 15;
        public UnityEvent StateChanged;

		private Stack<UIStateInvocation> stack = new Stack<UIStateInvocation>();

		public UIStateInvocation Current {
			get { return stack.Peek(); }
		}

        public int Depth {
            get { return stack.Count; }
        }

		void Awake() {
			uiRoot.SetActive(true);
			for(int i = 0; i < uiRoot.transform.childCount; i++) {
				uiRoot.transform.GetChild(i).gameObject.SetActive(false);
			}
		}

        public void PushState(string uri) {
            PushState(uri, new Dictionary<string, string>());
        }

        public void PushState(string url, Dictionary<string, string> parameters) {
            if (stack.Count > 0 && Current.Url == url) {
                Debug.LogWarning("State " + url + " is already visible.");
                return;
            }

            foreach(string key in parameters.Keys) {
                url = url.Replace("{#" + key + "}", parameters[key]);
            }

            UIStateInvocation invocation = UrlProcessor.MatchState(states, url);
            foreach (string key in parameters.Keys) {
                if(!invocation.Parameters.ContainsKey(key)) {
                    invocation.Parameters.Add(key, parameters[key]);
                }
            }

            if (invocation == null) {
				Debug.LogWarning("Invalid state uri: " + url);
				return;
			}

            invocation.Url = url;

            Debug.Log("Push state " + url);

			InvokeBeforeExit();
			HideCurrentState();

			if(stack.Count > 0 && Current.State.Modal) {
				Pop();
			}

			Push(invocation);
			ShowCurrentState();
            InvokeOnEnter();
            InvokeChanged();
        }

        public void ExitAndPushState(string uri) {
            ExitAndPushState(uri, new Dictionary<string, string>());
        }


        public void ExitAndPushState(string uri, Dictionary<string, string> parameters) {
			InvokeBeforeExit();
			HideCurrentState();
			Pop();
			PushState(uri, parameters);
		}

		public void Back() {
			InvokeBeforeExit();
			HideCurrentState();
			Pop();
			ShowCurrentState();
            InvokeOnEnter();
            InvokeChanged();
        }

		private void Push(UIStateInvocation invocation) {
			stack.Push(invocation);

			if (stack.Count <= maxStackDepth)
            	return;

			stack = new Stack<UIStateInvocation>(stack.ToArray().Take(maxStackDepth)); 
		}

		private void Pop() {
			stack.Pop();
		}

		private void HideCurrentState() {
			if(stack.Count > 0) {
				UIState oldState = Current.State;

				foreach(GameObject hide in oldState.VisibleObjects) {
					hide.SetActive(false);
				}
			}
		}

		private void InvokeBeforeExit() {
			if(stack.Count > 0) {
				UIState oldState = Current.State;

				if(oldState.OnBeforeExit != null) {
                    oldState.OnBeforeExit.Invoke();
				}
			}
		}

        private void InvokeOnEnter() {
            if (Current.State.OnEnter != null)
                Current.State.OnEnter.Invoke();
        }

        private void InvokeChanged() {
            if (StateChanged != null)
                StateChanged.Invoke();
        }

        private void ShowCurrentState() {
			UIState state = Current.State;

			foreach(GameObject show in state.VisibleObjects) {
				show.SetActive(true);
			}
		}

	}
}
