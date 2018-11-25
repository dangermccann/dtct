using System;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Map;
using DCTC.Model;

// TODO: Namespace

public class Level : MonoBehaviour {

	public MapConfiguration MapConfiguration;

    void Awake() {
        // TODO: move all of this somewhere else!!!!
        int seed = 100;
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        MapGenerator generator = new MapGenerator(new System.Random(seed));
        MapConfiguration = generator.Generate();
        Game.CurrentLevel = this;

        stopwatch.Stop();
        Debug.Log("Startup time: " + stopwatch.ElapsedMilliseconds + "ms");
    }
}

