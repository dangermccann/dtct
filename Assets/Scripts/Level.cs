using System;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Map;
using DCTC.Model;

// TODO: Namespace

public class Level : MonoBehaviour {

	public MapConfiguration MapConfiguration;

    void Awake() {
        int neighborhoodWidth = 60; 
        int neighborhoodHeight = 60;
        MapConfiguration config = new MapConfiguration(neighborhoodWidth * 2, neighborhoodHeight * 2);


        NameGenerator ng = new NameGenerator(new System.Random());
        ng.RandomMajorStreet();


        config.CreateTiles();


        Neighborhood template = new Neighborhood(config, neighborhoodWidth, neighborhoodHeight);
        template.CreateStraightRoad(new TilePosition(0,  0), new TilePosition(neighborhoodWidth,  0), Orientation.Horizontal);
        template.CreateStraightRoad(new TilePosition(0, 20), new TilePosition(neighborhoodWidth, 20), Orientation.Horizontal);
        template.CreateStraightRoad(new TilePosition(0, 40), new TilePosition(neighborhoodWidth, 40), Orientation.Horizontal);

        template.CreateStraightRoad(new TilePosition( 0, 0), new TilePosition( 0, neighborhoodHeight), Orientation.Vertical);
        template.CreateStraightRoad(new TilePosition(20, 0), new TilePosition(20, neighborhoodHeight), Orientation.Vertical);
        template.CreateStraightRoad(new TilePosition(40, 0), new TilePosition(40, neighborhoodHeight), Orientation.Vertical);

        config.Neighborhoods.Add(template);

        MapConfiguration = config;
        Game.CurrentLevel = this;
    }
}

