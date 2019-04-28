using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public class TechnologyTree : MonoBehaviour {
        enum TechnologyState {
            Unavailable,
            Available,
            Discovered,
            Researching
        }

        public GameObject CardPrefab;

        Transform container;
        GameController gameController;
        ToggleGroup toggleGroup;
        GameObject currentProgress = null;
        bool redraw = false;

        private static Color NormalColor = Utilities.CreateColor(0x637690);
        private static Color DiscoveredColor = Utilities.CreateColor(0x94B7A4);
        private static Color UnavailableColor = Utilities.CreateColor(0x9EA3AB);

        void Awake() {
            container = transform.Find("ScrollingListHorizontal/Viewport/Content");
            gameController = GameController.Get();
            toggleGroup = GetComponent<ToggleGroup>();
        }

        private void OnEnable() {
            Redraw();
            if(gameController.Game != null)
                gameController.Game.Player.TechnologyCompleted += Player_TechnologyCompleted;
        }

        private void OnDisable() {
            if (gameController.Game != null)
                gameController.Game.Player.TechnologyCompleted -= Player_TechnologyCompleted;
        }

        private void Update() {
            if (redraw) {
                Redraw();
                redraw = false;
            }

            if (currentProgress != null) {
                Company company = gameController.Game.Player;
                if (company.CurrentlyResearching != null) {
                    SetProgress(company.CurrentlyResearching, currentProgress);
                }
            }
        }

        private void Player_TechnologyCompleted() {
            redraw = true;
        }

        public void Redraw() {
            if (gameController.Game == null)
                return;

            Utilities.Clear(container);
            currentProgress = null;

            Technology graph = gameController.Game.TechnologyGraph;
            DrawTech(graph, 0, 0);

            //  TODO: make dynamic
            container.GetComponent<RectTransform>().sizeDelta = new Vector2(1700, 300);
        }

        const float xSpacing = 250;
        const float ySpacing = 65;
        const float leftMargin = 20;

        private Vector2 CardLocation(int xLevel, int yLevel) {
            return new Vector2((xLevel * xSpacing) + leftMargin, -1 * yLevel * ySpacing);
        }

        private GameObject DrawTech(Technology tech, int xLevel, int yLevel) {
            GameObject go = Instantiate(CardPrefab, container);
            go.name = tech.ID;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = CardLocation(xLevel, yLevel);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1); 

            go.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = tech.ID;

            Toggle toggle = go.GetComponent<Toggle>();
            toggle.group = toggleGroup;
            toggle.onValueChanged.AddListener((val) => {
                if (val) {
                    gameController.Game.Player.CurrentlyResearching = tech.ID;
                    currentProgress = go.transform.Find("Progress").gameObject;
                }
                else if(gameController.Game.Player.CurrentlyResearching == tech.ID) {
                    gameController.Game.Player.CurrentlyResearching = null;
                }
            });

            if (xLevel == 0)
                SetCardState(go, GetState(tech, null));

            int i = 0;
            foreach(Technology child in tech.DependantTechnologies) {
                GameObject childGO = DrawTech(child, xLevel + 1, yLevel + i);
                SetCardState(childGO, GetState(child, tech));

                //DrawConnection(Center(rt.anchoredPosition, rt), 
                //               Center(CardLocation(xLevel + 1, yLevel + i), rt));
                i++;
            }

            return go;
        }

        /*
        private Vector2 Center(Vector2 pos, RectTransform obj) {
            return new Vector2(pos.x + obj.sizeDelta.x / 2, pos.y - obj.sizeDelta.y / 2);
        }

        private void DrawConnection(Vector2 from, Vector2 to) {
            GameObject go = new GameObject("line", typeof(Image));
            go.GetComponent<Image>().color = Color.white;
            go.transform.SetParent(container, false);
            RectTransform rc = go.GetComponent<RectTransform>();
            rc.anchorMin = new Vector2(0, 1);
            rc.anchorMax = new Vector2(0, 1);
            rc.pivot = Vector2.zero;
            rc.sizeDelta = new Vector2(Vector2.Distance(from, to), 3);
            rc.anchoredPosition = from;
            rc.localEulerAngles = new Vector3(0, 0, -1 * Vector2.Angle(from, to));
            rc.SetAsFirstSibling();
        }
        */

        private TechnologyState GetState(Technology tech, Technology parent) {
            Company company = gameController.Game.Player;
            if (company.HasTechnology(tech.ID))
                return TechnologyState.Discovered;
            if (parent != null && company.HasTechnology(parent.ID))
                return TechnologyState.Available;
            if (company.CurrentlyResearching == tech.ID)
                return TechnologyState.Researching;

            return TechnologyState.Unavailable;
        }



        private void SetCardState(GameObject card, TechnologyState state) {
            switch(state) {
                case TechnologyState.Available:
                    card.GetComponent<Toggle>().enabled = true;
                    card.GetComponent<Toggle>().isOn = false;
                    card.GetComponent<Image>().color = NormalColor;
                    SetProgress(card.name, card.transform.Find("Progress").gameObject);
                    break;
                case TechnologyState.Discovered:
                    card.GetComponent<Toggle>().enabled = false;
                    card.GetComponent<Toggle>().isOn = false;
                    card.GetComponent<Image>().color = DiscoveredColor;
                    card.transform.Find("Progress").gameObject.SetActive(false);
                    break;
                case TechnologyState.Researching:
                    card.GetComponent<Toggle>().enabled = true;
                    card.GetComponent<Toggle>().isOn = true;
                    card.GetComponent<Image>().color = NormalColor;
                    card.transform.Find("Progress").gameObject.SetActive(true);
                    currentProgress = card.transform.Find("Progress").gameObject;
                    break;
                case TechnologyState.Unavailable:
                    card.GetComponent<Toggle>().enabled = false;
                    card.GetComponent<Toggle>().isOn = false;
                    card.GetComponent<Image>().color = UnavailableColor;
                    card.transform.Find("Progress").gameObject.SetActive(false);
                    break;
            }
        }

        void SetProgress(string techId, GameObject card) {
            Technology tech = gameController.Game.TechnologyGraph.Find(techId);
            float progress = gameController.Game.Player.Technologies[techId] / tech.Cost;

            card.SetActive(true);
            Vector2 parentSize = card.transform.parent.GetComponent<RectTransform>().sizeDelta;
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(parentSize.x * progress, parentSize.y);
        }
    }
}
