using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileAnchor : MonoBehaviour {

    private static WorldGenerator worldGenerator = WorldGenerator.worldGenerator;

    [Header("Tile Setup")]
    //Key: direction to the socket, Value: Socket
    public Dictionary<Vector3Int, SocketType> sockets = new();
    public Vector3Int cellIndex;
    public List<TileType> possibleTiles = new();
    private List<SocketRule> possibleRules = new();
    
    //Final Tile Results
    public bool isCollapsed = false;
    public TileType collapsedType = TileType.Undefined;
    public SocketRule collapsedRule = SocketRule.UndefinedRule();

    //Path and Selection
    private List<PathAnchor> pathAnchors = new();

    private void Awake() {
        worldGenerator = WorldGenerator.worldGenerator;

        Setup();
    }

    private void Start() {
        
    }

    public void Reset() {
        possibleTiles.Clear();
        possibleRules.Clear();
        pathAnchors.Clear();
        isCollapsed = false;
        collapsedType = TileType.Undefined;
        collapsedRule = SocketRule.UndefinedRule();

        Setup();
    }

    private void Setup() {
        //Add every tileType to possibleTiles except Undefined and Slope
        foreach (TileType type in Enum.GetValues(typeof(TileType))) {
            if (type != TileType.Undefined && type != TileType.Slope) possibleTiles.Add(type);
        }
        //Add every socketrule to possibleRules
        foreach (SocketRule rule in worldGenerator.socketRulebook.Values) {
            possibleRules.Add(rule);
        }
        //Set all sockets to undefined
        Vector3Int[] directions = {Vector3Int.right, Vector3Int.up, Vector3Int.forward, Vector3Int.left, Vector3Int.down, Vector3Int.back};
        foreach (Vector3Int direction in directions) {
            sockets[direction] = SocketType.Undefined;
        }
    }

    //Set the possibleRules to the rules has highest score
    public void LookupRules() {
        //Socket directions
        Vector3Int[] directions = {Vector3Int.right, Vector3Int.up, Vector3Int.forward, Vector3Int.left, Vector3Int.down, Vector3Int.back};
        List<(SocketRule, int)> ruleScore = new();
        
        //If sockets matches score++
        foreach (SocketRule rule in possibleRules) {
            int score = 0;
            if (rule.px == sockets[directions[0]]) score++;
            if (rule.py == sockets[directions[1]]) score++;
            if (rule.pz == sockets[directions[2]]) score++;
            if (rule.nx == sockets[directions[3]]) score++;
            if (rule.ny == sockets[directions[4]]) score++;
            if (rule.nz == sockets[directions[5]]) score++;
            ruleScore.Add((rule, score));
        }

        //Get all options in highest score
        int maxScore = ruleScore.Max(rule => rule.Item2);
        possibleTiles = ruleScore.Where(rule => rule.Item2 == maxScore).Select(rule => rule.Item1.tileType).ToList();
        possibleRules = ruleScore.Where(rule => rule.Item2 == maxScore).Select(rule => rule.Item1).ToList();
    }

    //Update sockets after collapsed into a tile
    public void UpdateSockets() {
        //Check if tile is collapsed
        if (collapsedType == TileType.Undefined) return;

        //Clear both possibles
        possibleTiles.Clear();
        possibleRules.Clear();

        //Set all sockets to the collapsed rule
        sockets[Vector3Int.right] = collapsedRule.px;
        sockets[Vector3Int.up] = collapsedRule.py;
        sockets[Vector3Int.forward] = collapsedRule.pz;
        sockets[Vector3Int.left] = collapsedRule.nx;
        sockets[Vector3Int.down] = collapsedRule.ny;
        sockets[Vector3Int.back] = collapsedRule.nz;
    }

    public void Collapse() {
        if (possibleRules.Count == 0) {
            collapsedType = TileType.Undefined;
            collapsedRule = SocketRule.UndefinedRule();
        }
        else {
            //Decide a random tiletype to collapse into
            collapsedRule = possibleRules[UnityEngine.Random.Range(0, possibleRules.Count)];
            collapsedType = collapsedRule.tileType;
        }
        
        isCollapsed = true;

        //Summon corresponding tile
        GameObject tile = Instantiate(worldGenerator.tilePrefabs[(int)collapsedType], transform.position, Quaternion.Euler(0, collapsedRule.rotation * 90, 0));
        LinkPathToTile(tile);
    }

    private void LinkPathToTile(GameObject tilePrefab) {
        pathAnchors = tilePrefab.GetComponentsInChildren<PathAnchor>().ToList();
        foreach (PathAnchor pathAnchor in pathAnchors) {
            pathAnchor.EnterCellIndex(transform);
        }
    }
}