using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DCTC.UI {
    public abstract class EnumToggleGroup : MonoBehaviour {

        private List<Toggle> toggles = new List<Toggle>();

        void Start() {
            Init();

            for (int i = 0; i < transform.childCount; i++) {
                toggles.Add(GetToggle(i));
                AddListener(i);
            }

            UpdateToggles();
        }

        protected abstract void Init();
        protected abstract int GetValue();
        protected abstract void SetValue(int value);

        protected Toggle GetToggle(int index) {
            return transform.GetChild(index).gameObject.GetComponent<Toggle>();
        }

        private void AddListener(int index) { 
            toggles[index].onValueChanged.AddListener((val) => {
                if (val) {
                    SetValue(index);
                }
                else {
                    if(toggles.Count(t => t.isOn) == 0) {
                        toggles[index].isOn = true;
                    }
                }
            });
        }

        protected void UpdateToggles() {
            int value = GetValue();
            for(int i = 0; i < toggles.Count; i++) { 
                toggles[i].isOn = (value == i);
            }
        }
    }
}