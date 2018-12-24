using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Map;

namespace DCTC.UI {
    public class OverlayToggles : EnumToggleGroup {
        public ThreeDMap Map;

        protected override void Init() {
            Map.OverlayModeChanged += UpdateToggles;
        }

        protected override int GetValue() {
            return (int) Map.OverlayMode;
        }

        protected override void SetValue(int value) {
            if(Map.OverlayMode != (OverlayMode)value)
                Map.OverlayMode = (OverlayMode) value;
        }
    }
}
