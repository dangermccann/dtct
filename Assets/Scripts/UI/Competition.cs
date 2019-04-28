using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public class Competition : MonoBehaviour {
        public GameObject AttributesPrefab;
        private Transform container;
        private GameController gameController;
        private int index = 0;
        private List<CompanyAttributes> attributes = null;
        private float cooldown = 0.25f;

        void Awake() {
            container = transform.Find("Container");
            gameController = GameController.Get();
        }

        private void OnEnable() {
            Redraw();
        }

        void Redraw() {
            if (gameController.Game == null)
                return;

            Utilities.Clear(container);

            attributes = new List<CompanyAttributes>();

            foreach (Company company in gameController.Game.Companies) {
                GameObject go = Instantiate(AttributesPrefab, container);
                CompanyAttributes atts = go.GetComponent<CompanyAttributes>();
                attributes.Add(atts);
                atts.Company = company;
            }
        }

        private void Update() {
            cooldown -= Time.deltaTime;

            if(cooldown < 0) {
                cooldown = 0.25f;

                attributes[index % attributes.Count].Redraw();
                index++;
            }
        }

    }
}