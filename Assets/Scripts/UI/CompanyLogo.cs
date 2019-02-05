using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.UI {
    public class CompanyLogo : MonoBehaviour {
        private Company company;
        public Company Company {
            get { return company; }
            set {
                company = value;
                Redraw();
            }
        }

        private GameController gameController;

        private void OnEnable() {
            gameController = GameController.Get();
            Redraw();
        }

        public void Redraw() {
            Company c = Company;
            if(c == null) {
                if(gameController.Game != null)
                    c = gameController.Game.Player;
            }

            if(c != null)
                transform.Find("Image").GetComponent<Image>().sprite = SpriteController.Get().GetSprite(c.Name);
        }

    }

}