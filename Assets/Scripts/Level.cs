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

        MapGenerator generator = new MapGenerator(new System.Random(seed));
        MapConfiguration = generator.Generate();
        Game.CurrentLevel = this;
    }
}

