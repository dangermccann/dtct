using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DCTC.Controllers;

namespace DCTC.UI {
    public class LoadingUI : MonoBehaviour {

        private void OnEnable() {
            string company = StateController.Get().Current.Parameters["Company"];
            Sprite sprite = SpriteController.Get().GetSprite(company);
            transform.Find("CompanyLogo").GetComponent<Image>().sprite = sprite;
        }
    }

}