using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCTC.Controllers {
    public class SpriteController : MonoBehaviour {

        public static SpriteController Get() {
            return GameObject.FindGameObjectWithTag("GameController").GetComponent<SpriteController>();
        }

        private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        void Awake() {
            LoadResources("Textures");
        }

        public Sprite GetSprite(string sprite) {
            if (sprites.ContainsKey(sprite))
                return sprites[sprite];
            else return null;
        }

        public bool HasSprite(string sprite) {
            return sprites.ContainsKey(sprite);
        }

        private void LoadResources(string directory) {
            Sprite[] textures = Resources.LoadAll<Sprite>(directory);
            foreach (Sprite s in textures) {
                if (sprites.ContainsKey(s.name)) {
                    Debug.LogWarning("Duplicate spirte: " + s.name);
                    continue;
                }
                sprites.Add(s.name, s);
            }
        }

    }
}
