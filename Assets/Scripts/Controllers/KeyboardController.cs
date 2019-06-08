using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace DCTC.Controllers {
    public class KeyboardController : MonoBehaviour {

        GameController gameController;
        StateController stateController;

        private void Start() {
            gameController = GameController.Get();
            stateController = StateController.Get();
        }

        void Update() {
            if (IsTextInputActive())
                return;

            if(Input.GetKeyDown(KeyCode.Escape)) {
                if (stateController.Current.State.UrlPattern == States.Map) {
                    gameController.Pause();
                    stateController.PushState(States.GameMenu);
                } else if (stateController.Current.State.UrlPattern == States.Loading ||
                           stateController.Current.State.UrlPattern == States.Title) {
                    // Ignore
                } else if (stateController.Current.State.UrlPattern == States.NewGame) {
                    StateController.Get().ExitAndPushState(States.Title);
                } else if (stateController.Current.State.UrlPattern == States.GameMenu) {
                    stateController.Back();
                    gameController.Unpause();
                } else {
                    stateController.Back();
                }
            }

            if(Input.GetKeyDown(KeyCode.Space)) {
                if (gameController.GameSpeed == GameSpeed.Pause)
                    gameController.Unpause(true);
                else
                    gameController.Pause();
            }

            if (Input.GetKeyDown(KeyCode.F2)) {
                if(gameController.Game != null)
                    StateController.Get().PushState(States.Workforce);
            }

            if (Input.GetKeyDown(KeyCode.F4)) {
                if (gameController.Game != null)
                    StateController.Get().PushState(States.Finance);
            }

            if (Input.GetKeyDown(KeyCode.F12))
                gameController.Game.Player.Money += 50000;
        }

        public static bool IsTextInputActive() {
            GameObject current = EventSystem.current.currentSelectedGameObject;
            if (current != null) {
                UnityEngine.UI.InputField input = current.GetComponent<UnityEngine.UI.InputField>();
                if (input != null) {
                    return true;
                }
            }
            return false;
        }

    }
}
