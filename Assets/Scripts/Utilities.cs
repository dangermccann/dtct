using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions {


    public static void AddMany<T>(this HashSet<T> set, IEnumerable<T> items) {
        foreach (T item in items) {
            set.Add(item);
        }
    }

    public static void AddMany<K, V>(this Dictionary<K, V> set, Dictionary<K, V> items) {
        foreach (K key in items.Keys) {
            set.Add(key, items[key]);
        }
    }

    public static void AddMany<K, E, V>(this Dictionary<K, V> set, Dictionary<K, E> items) where E : V {
        foreach (K key in items.Keys) {
            set.Add(key, items[key]);
        }
    }

    public static void RemoveMany<T>(this HashSet<T> set, IEnumerable<T> items) {
        foreach (T item in items) {
            set.Remove(item);
        }
    }

    public static void AddManySafely<T>(this HashSet<T> set, IEnumerable<T> items) {
        foreach (T item in items) {
            if(!set.Contains(item))
                set.Add(item);
        }
    }

    public static void RemoveAll<K, V>(this IDictionary<K, V> dict, Func<K, V, bool> predicate) {
        foreach (var key in dict.Keys.ToArray().Where(key => predicate(key, dict[key])))
            dict.Remove(key);
    }

    public static void Shuffle<T>(this IList<T> list, System.Random random) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static IDictionary<K, V> Map<K, V>(this IEnumerable<K> items, Func<K, V> predicate) {
        Dictionary<K, V> dict = new Dictionary<K, V>(items.Count());
        foreach(K item in items) {
            dict.Add(item, predicate(item));
        }
        return dict;
    }

    public static Vector2 Clamp(this Vector2 _this, float minX, float minY, float maxX, float maxY) {
        return _this.Clamp(new Vector2(minX, minY), new Vector2(maxX, maxY));
    }

    public static Vector2 Clamp(this Vector2 _this, Vector2 min, Vector2 max) {
        _this.x = Mathf.Clamp(_this.x, min.x, max.x);
        _this.y = Mathf.Clamp(_this.y, min.y, max.y);
        return _this;
    }


    public static Vector2 AsVector(this DCTC.Map.TilePosition pos) {
        return new Vector2(pos.x, pos.y);
    }
}



public class Easing {
    public static Vector3 ExpoEaseOut(float currentTime, Vector3 startValue, Vector3 changeInValue, float duration) {
        return changeInValue * (-Mathf.Pow(2, -10 * currentTime / duration) + 1) + startValue;
    }

    public static Vector3 QuadEaseOut(float currentTime, Vector3 startValue, Vector3 changeInValue, float duration) {
        currentTime /= duration;
        return -changeInValue * currentTime * (currentTime - 2) + startValue;
    }
}

public static class RandomUtils {
    public static T RandomThing<T>(HashSet<T> things, System.Random random) {
        return RandomThing<T>(new List<T>(things), random);
    }

    public static T RandomThing<T>(IList<T> things, System.Random random) {
        return things[random.Next(0, things.Count)];
    }

    public static T RandomEnumValue<T>() {
        return RandomEnumValue<T>(new System.Random(DateTime.Now.Millisecond));
    }

    public static T RandomEnumValue<T>(System.Random random) {
        List<T> all = Enum.GetValues(typeof(T)).Cast<T>().ToList();
        return RandomThing(all, random);
    }

    public static T RoundRobinEnum<T>(int index) {
        List<T> all = Enum.GetValues(typeof(T)).Cast<T>().ToList();
        return all[index % all.Count];
    }

    public static decimal Jitter(decimal value, System.Random rand, decimal jitterPercent) {
        return value + (((decimal)rand.NextDouble() - 0.5m) * (value * jitterPercent / 100m));
    }

    public static decimal RandomDecimal(decimal min, decimal max, System.Random rand) {
        return (decimal)rand.NextDouble() * (max - min) + min;
    }

    public static float RandomFloat(float min, float max, System.Random rand) {
        return (float)rand.NextDouble() * (max - min) + min;
    }

    public static T Choice<T>(System.Random random, decimal chance, T first, T second) {
        if ((decimal)random.NextDouble() <= chance)
            return first;
        else return second;
    }

    public static DateTime RandomDate(System.Random random, int startYear, int endYear) {
        DateTime start = new DateTime(startYear, 1, 1);
        int range = (endYear - startYear) * 365;
        return start.AddDays(random.Next(range));
    }

    public static float LinearLikelihood(float min, float max, float current) {
        return Mathf.Clamp01( (current - min) / (max - min) );
    }

    /// <summary>
    /// Represents a chance that a certain outcome will become true.
    /// </summary>
    /// <param name="chance">Value between 0 and 1 indicating the likelihood that the chance will be true.</param>
    /// <returns></returns>
    public static bool Chance(double chance) {
        return Chance(new System.Random(), chance);
    }

    public static bool Chance(System.Random rand, double chance) {
        return (rand.NextDouble() < chance);
    }

    /// <summary>
    ///   Generates normally distributed numbers. 
    /// </summary>
    /// <param name="random">The Random object</param>
    /// <param name = "mu">Mean of the distribution</param>
    /// <param name = "sigma">Standard deviation</param>
    /// <returns></returns>
    public static decimal Gaussian(System.Random random, double mu = 0, double sigma = 1) {
        double u1 = random.NextDouble();
        double u2 = random.NextDouble();

        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                            Math.Sin(2.0 * Math.PI * u2);

        var randNormal = mu + sigma * randStdNormal;

        return (decimal) randNormal;
    }
    
}


public static class Utilities {
    public static Color CreateColor(int hex) {
        return new Color(
            ((hex >> 16) & 0xff) / 255f,
            ((hex >> 8) & 0xff) / 255f,
            (hex & 0xff) / 255f);
    }


    public static void RecursivelyApply(GameObject go, Action<GameObject> lambda) {
        lambda.Invoke(go);

        for (int i = 0; i < go.transform.childCount; i++) {
            lambda.Invoke(go.transform.GetChild(i).gameObject);
        }
    }

    public static void Clear(Transform transform) {
        Clear(transform, 0, 0);
    }

    public static void Clear(Transform transform, int offsetStart, int offsetEnd) {
        for (int i = transform.childCount - offsetEnd - 1; i >= offsetStart; i--) {
            GameObject go = transform.GetChild(i).gameObject;
            SafeDestroy(go);
        }
    }

    public static Vector3 WorldToRectTransformLocal(Vector3 point, RectTransform rt) {
        Vector3 local = rt.transform.InverseTransformPoint(point);
        local = new Vector3(local.x + rt.pivot.x * rt.rect.width,
            local.y + rt.pivot.y * rt.rect.height, local.z);

        return local;
    }

    public static Vector3 RectTransformLocalToWorld(Vector3 local, RectTransform rt) {
        local = new Vector3(local.x - rt.pivot.x * rt.rect.width,
            local.y - rt.pivot.y * rt.rect.height, local.z);
        return rt.transform.TransformPoint(local);
    }

    public static T InstantiateComponent<T>(UnityEngine.Object prefab, Transform parent) where T : Component {
        GameObject go = (GameObject)GameObject.Instantiate(prefab);
        go.transform.SetParent(parent, false);
        return go.GetComponent<T>();
    }

    public static void MaximizeBoxCollider(GameObject go) {
        MaximizeBoxCollider(go, go.GetComponent<RectTransform>());
    }

    public static void MaximizeBoxCollider(GameObject go, RectTransform rt) {
        BoxCollider2D bc = go.GetComponent<BoxCollider2D>();

        bc.offset = new Vector2((rt.rect.width / 2) - (rt.rect.width * rt.pivot.x), (rt.rect.height / 2) - (rt.rect.height * rt.pivot.y));
        bc.size = new Vector2(rt.rect.width, rt.rect.height);
    }

    public static void MatchParentDimensions(GameObject obj, GameObject parent) {
        MatchParentDimensions(obj, parent, new Vector2(0, 0));
    }

    public static void MatchParentDimensions(GameObject obj, GameObject parent, Vector2 margin) {
        RectTransform rt = obj.GetComponent<RectTransform>();
        RectTransform parentRT = parent.GetComponent<RectTransform>();
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, margin.x, parentRT.rect.width - 2.0f * margin.x);
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, margin.y, parentRT.rect.height - 2.0f * margin.y);
    }

    public static float Normalize(float val, float min, float max) {
        float range = (max - min);
        if (range == 0)
            return 0f;

        return (val - min) / range;
    }

    public static bool AreClose(float a, float b) {
        return Mathf.Abs(a - b) < 0.0001f;
    }

    public static bool GreaterThanOrCloseTo(float a, float b) {
        return AreClose(a, b) || (a >= b);
    }

    public static bool LessThanOrCloseTo(float a, float b) {
        return AreClose(a, b) || (a <= b);
    }

    public static void SafeDestroy(UnityEngine.Object obj) {
        if (Application.isEditor)
            UnityEngine.Object.DestroyImmediate(obj);
        else
            UnityEngine.Object.Destroy(obj);
    }

    public static GameObject FindOrInstantiate(string name, Transform parent, GameObject prefab) {
        Transform t = parent.Find(name);
        if (t != null)
            return t.gameObject;
        else {
            GameObject go = (GameObject)GameObject.Instantiate(prefab);
            go.transform.SetParent(parent, false);
            go.name = name;
            return go;
        }
    }

    public static bool FindAndDestroy(string name, Transform parent) {
        Transform t = parent.Find(name);
        if (t != null) {
            SafeDestroy(t.gameObject);
            return true;
        }
        return false;
    }

    public static int ClearRemaining(string prefix, Transform parent, int start) {
        int i = start;
        int count = 0;
        while (FindAndDestroy(prefix + i.ToString(), parent) == true) {
            i++;
            count++;
        }

        return count;
    }


    public static Transform FindChildRecursive(this Transform aParent, string aName) {
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent) {
            result = child.FindChildRecursive(aName);
            if (result != null)
                return result;
        }
        return null;
    }

    public static void DestroyAllChildren(this Transform aParent) {
        for (int i = aParent.childCount - 1; i >= 0; i--) {
            SafeDestroy(aParent.GetChild(i).gameObject);
        }
    }

    public static void AssureMaterialPresent(this MeshRenderer renderer, Material material) {
        List<Material> materials = new List<Material>(renderer.materials);
        int found = materials.Where(m => (m.name.StartsWith(material.name))).Count();
        if (found == 0) {
            materials.Insert(0, material);
            renderer.materials = materials.ToArray();
        }
    }

    public static void AssureMaterialAbsent(this MeshRenderer renderer, Material material) {
        List<Material> materials = new List<Material>(renderer.materials);
        int found = materials.FindIndex(m => m.name.StartsWith(material.name));
        if (found != -1) {
            materials.RemoveAt(found);
            renderer.materials = materials.ToArray();
        }
    }

    public static T ParseEnum<T>(string value) {
        return (T)Enum.Parse(typeof(T), value);
    }

    public static float StandardDeviation(IEnumerable<float> values) {
        float mean = values.Average();
        float squares = values.Sum(v => Mathf.Pow(v - mean, 2));
        return Mathf.Sqrt(squares / values.Count());
    }

}

[System.Serializable]
public class SerializableColor {
    public float R;
    public float G;
    public float B;
    public float A;
    public SerializableColor(Color color) {
        R = color.r;
        G = color.g;
        B = color.b;
        A = color.a;
    }
    public Color GetColor() {
        return new Color(R, G, B, A);
    }
}

/// <summary>
/// Since unity doesn't flag the Vector3 as serializable, we
/// need to create our own version. This one will automatically convert
/// between Vector3 and SerializableVector3
/// </summary>
[System.Serializable]
public struct SerializableVector3 {
    /// <summary>
    /// x component
    /// </summary>
    public float x;

    /// <summary>
    /// y component
    /// </summary>
    public float y;

    /// <summary>
    /// z component
    /// </summary>
    public float z;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    /// <param name="rZ"></param>
    public SerializableVector3(float rX, float rY, float rZ) {
        x = rX;
        y = rY;
        z = rZ;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }

    /// <summary>
    /// Automatic conversion from SerializableVector3 to Vector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Vector3(SerializableVector3 rValue) {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    /// <summary>
    /// Automatic conversion from Vector3 to SerializableVector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableVector3(Vector3 rValue) {
        return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    }
}