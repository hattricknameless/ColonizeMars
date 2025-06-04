using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {
    
    public static WorldGenerator worldGenerator;
    private PathManager pathManager;
    
    [SerializeField] private TextAsset socketRulesAsset;
    public Dictionary<string, SocketRule> socketRulebook = new(); //Stores all the combinations of socket
    
    //Prefabs
    private GameObject tileAnchorPrefab; //TileAnchorPrefab
    public List<GameObject> tilePrefabs; //int(TileType)/Prefab
    private GameObject orePrefab;

    //Grid Settings
    public Vector3Int worldSize; //World size
    public Vector3 gridScale = new Vector3(2f, 1f, 2f); //Size of each tile object
    private Dictionary<Vector3Int, TileAnchor> worldGrid; //Key: cellIndex, Value: tileAnchor
    [SerializeField] private float oreGenerateRate; //What percentage of terrian is covered with ore

    //Collapse Process
    [SerializeField] private Vector3Int initialCollapse;
    private int collapsedCount;
    private HashSet<Vector3Int> uncollapsedTiles = new();
    private Coroutine collapseCoroutine;

    private void Awake() {
        worldGenerator = this;
        worldGrid = new();

        tileAnchorPrefab = Resources.Load("SystemPrefabs/TileAnchor") as GameObject;
        orePrefab = Resources.Load("SystemPrefabs/OrePrefab") as GameObject;

        LoadSocketRules();
    }

    private void Start() {
        pathManager = PathManager.pathManager;
        
        ReGenerateWorld();
    }

    private void FillAnchor() {
        //Fill the world with TileAnchor
        for (int x = 0; x < worldSize.x; x++) {
        for (int z = 0; z < worldSize.z; z++) {
        for (int y = 0; y < worldSize.y; y++) {
            Vector3Int cellIndex = new Vector3Int(x, y, z);
            GameObject anchor = Instantiate(tileAnchorPrefab, Vector3.Scale(new Vector3(x, y, z), gridScale), Quaternion.identity);
            //Fill the worldGrid references
            worldGrid[cellIndex] = anchor.GetComponent<TileAnchor>();
            worldGrid[cellIndex].cellIndex = cellIndex;

            //Fill the uncollapsed set
            uncollapsedTiles.Add(cellIndex);
        }
        }
        }
    }

    public void ReGenerateWorld() {
        if (collapseCoroutine == null) {
            collapseCoroutine = StartCoroutine(GenerateWorld());
        }
        else {
            Debug.Log("World is generating");
        }
    }

    private IEnumerator GenerateWorld() {
        //Destory all previous tiles
        GameObject[] existingTiles = GameObject.FindGameObjectsWithTag("TerrianTile");
        foreach (GameObject tile in existingTiles) Destroy(tile);
        GameObject[] existingOres = GameObject.FindGameObjectsWithTag("NatrualResources");
        foreach (GameObject ore in existingOres) Destroy(ore);
        
        pathManager.ResetDataStructure();
        yield return new WaitForEndOfFrame();
        
        if (collapsedCount == 0) {
            //If the world wasn't previously generated or game start
            FillAnchor();
        }
        else {
            //Reset value within all TileAnchor and add all index in uncollapsedTiles
            foreach (KeyValuePair<Vector3Int, TileAnchor> pair in worldGrid) {
                pair.Value.Reset();
                uncollapsedTiles.Add(pair.Key);
            }
        }
        yield return new WaitForEndOfFrame();

        Collapse(initialCollapse);
        while (uncollapsedTiles.Count > 0) {
            Collapse(NextCollapseTile());
        }
        Debug.Log("Collapse complete");
        yield return new WaitForEndOfFrame();

        //Register all pathAnchors
        List<PathAnchor> pathAnchors = FindObjectsOfType<PathAnchor>().ToList();
        foreach (PathAnchor pathAnchor in pathAnchors) {
            pathManager.RegisterToPathGrid(pathAnchor);
        }
        Debug.Log($"All pathAnchor count: {pathAnchors.Count()}, Registered: {pathManager.pathGrid.Count}");
        yield return new WaitForEndOfFrame();

        //Add Ores to the Terrian
        GenerateOre();
        Debug.Log($"Ore generation complete");

        collapseCoroutine = null;
        yield return null;
    }

    private void LoadSocketRules() {
        if (socketRulesAsset == null) {
            Debug.Log("SocketRuleAsset load failed");
            return;
        }

        InputRulebook importedRules = JsonUtility.FromJson<InputRulebook>(socketRulesAsset.text);
        
        //temp: each InputRule in the InputRulebook
        foreach (InputRule temp in importedRules.rules) {
            socketRulebook[temp.rule] = SocketRule.ConvertFromInput(temp);
        }
        Debug.Log($"Total rules: {socketRulebook.Count}");
    }

    private void Collapse(Vector3Int rootTile) {
        TileAnchor rootAnchor = worldGrid[rootTile];
        //Set a tile from possibleTiles (add weighted function if possible)
        rootAnchor.Collapse();
        rootAnchor.UpdateSockets();

        //Update data within WorldGenerator
        collapsedCount++;
        uncollapsedTiles.Remove(rootTile);

        //Update each adjacent tile's socket types
        Vector3Int[] directions = {Vector3Int.right, Vector3Int.up, Vector3Int.forward, Vector3Int.left, Vector3Int.down, Vector3Int.back};
        foreach (Vector3Int direction in directions) {
            Vector3Int targetTile = rootTile + direction;
            if (worldGrid.TryGetValue(targetTile, out TileAnchor selectedTile)) {
                //Check if targetTile isCollapsed
                if (selectedTile.isCollapsed) continue;
                
                //Set the counterpart socket to the same socketType
                selectedTile.sockets[direction * -1] = worldGrid[rootTile].sockets[direction];

                //Trigger LookupRules to update its possibleRules
                selectedTile.LookupRules();
            }
            else {
                //Can't find a tile
            }
        }
    }

    private Vector3Int NextCollapseTile() {
        //Find the anchor with least possible tiles
        List<(Vector3Int, int)> leastPossibleTile = new();
        foreach (Vector3Int tile in uncollapsedTiles) {
            leastPossibleTile.Add((tile, worldGrid[tile].possibleTiles.Count));
        }
        int minPossibility = leastPossibleTile.Min(count => count.Item2);
        return leastPossibleTile.Where(tile => tile.Item2 == minPossibility).OrderBy(tile => tile.Item1.y).Select(tile => tile.Item1).ToList().First();
    }

    private void GenerateOre() {
        int cellCount = pathManager.pathGrid.Count();

        //Shuffle list generated by Copilot
        List<int> indices = Enumerable.Range(0, cellCount).ToList();
        indices = indices.OrderBy(x => UnityEngine.Random.value).ToList();
        foreach (int index in indices.Take(Mathf.RoundToInt(cellCount * oreGenerateRate))) {
            NaturalResource ore = Instantiate(orePrefab, pathManager.pathGrid[pathManager.pathGrid.Keys.ToList()[index]].gameObject.transform.position, Quaternion.identity).GetComponent<NaturalResource>();
            ore.cellIndex = pathManager.pathGrid.Keys.ToList()[index];
            pathManager.RegisterToResourceGrid(ore);
        }
    }

    public void ConfirmWorld() {
        if (collapseCoroutine != null) {return;}
        Debug.Log($"{pathManager.pathGrid.Count} PathAnchor registered to pathGrid");
        Debug.Log($"{pathManager.resourceGrid.Count} Resources registered to resourceGrid");
        pathManager.RegisterToGraph();
        Debug.Log("Graph structure complete");
    }
}