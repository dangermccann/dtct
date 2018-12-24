using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Controllers;

namespace DCTC.UI {
    public class GameSpeedToggles : EnumToggleGroup {

        private GameController gameController;

        protected override void Init() {
            gameController = GameController.Get();
            gameController.SpeedChanged += UpdateToggles;
        }

        protected override int GetValue() {
            return (int) gameController.GameSpeed;
        }

        protected override void SetValue(int value) {
            if(gameController.GameSpeed != (GameSpeed)value)
                gameController.GameSpeed = (GameSpeed) value;
        }
    }
}
