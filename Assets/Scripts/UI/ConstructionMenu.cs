using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Controllers;

namespace DCTC.UI {
    public class ConstructionMenu : MonoBehaviour {

        public GameObject MainMenu, CablesMenu, NodesMenu;
        public SelectionController SelectionController;

        void Start() {
            for(int i = 0; i < MainMenu.transform.childCount; i++) {
                MainMenu.transform.GetChild(i).GetComponent<Toggle>().onValueChanged.AddListener(ToggleGroupChanged);
            }

            CablesMenu.SetActive(true);
            NodesMenu.SetActive(true);

            for (int i = 0; i < CablesMenu.transform.childCount; i++) {
                CablesMenu.transform.GetChild(i).GetComponent<Toggle>().onValueChanged.AddListener(ToggleGroupChanged);
            }

            for (int i = 0; i < NodesMenu.transform.childCount; i++) {
                NodesMenu.transform.GetChild(i).GetComponent<Toggle>().onValueChanged.AddListener(ToggleGroupChanged);
            }

            HideMenu(CablesMenu);
            HideMenu(NodesMenu);
        }

        void HideMenu(GameObject menu) {
            CanvasGroup cg = menu.GetComponent<CanvasGroup>();
            cg.alpha = 0;
            cg.blocksRaycasts = false;
        }

        void ShowMenu(GameObject menu) {
            CanvasGroup cg = menu.GetComponent<CanvasGroup>();
            cg.alpha = 1;
            cg.blocksRaycasts = true;
        }

        void ToggleGroupChanged(bool value) {
            GetTriState("Cables").IsSecondary = false;
            GetTriState("Nodes").IsSecondary = false;

            HideMenu(NodesMenu);
            HideMenu(CablesMenu);

            Toggle toggle = GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();
            if (toggle == null) {
                SelectionController.SetSelection(SelectionController.SelectionModes.None);
            } else {
                switch(toggle.name) {
                    case "Cables":
                        ShowMenu(CablesMenu);
                        break;

                    case "Nodes":
                        ShowMenu(NodesMenu);
                        break;

                    case "Destroy":
                        SelectionController.SetSelection(SelectionController.SelectionModes.Destroy);
                        break;

                    case "Copper":
                    case "Fiber":
                        SelectionController.SetSelection(SelectionController.SelectionModes.Cable);
                        GetTriState("Cables").IsSecondary = true;
                        break;

                    case "Small":
                    case "Advanced":
                    case "FiberNode":
                        SelectionController.SetSelection(SelectionController.SelectionModes.Node);
                        GetTriState("Nodes").IsSecondary = true;
                        break;
                }
            } 
        }

        TriStateToggle GetTriState(string name) {
            return MainMenu.transform.Find(name).GetComponent<TriStateToggle>();
        }
    }

}