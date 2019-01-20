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
                if (stateController.Current.State.UrlPattern == States.GameMenu)
                    stateController.Back();
                else
                    stateController.PushState(States.GameMenu);
            }

            if (Input.GetKeyDown(KeyCode.F1)) {
                if(gameController.Game != null)
                    StateController.Get().PushState(States.Workforce);
            }
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
