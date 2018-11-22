using UnityEngine;

namespace DCTC.Map {
    public abstract class AbstractMap : MonoBehaviour {
        public abstract void Init(MapConfiguration config);
        public abstract void StartDraw();
        public abstract void MoveCameraTo(TilePosition position);
    }
}
