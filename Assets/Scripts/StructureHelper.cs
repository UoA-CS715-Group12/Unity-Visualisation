using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureHelper : MonoBehaviour
{
    public BuildingType[] buildingTypes;
    public GameObject[] naturePrefabs;
    public bool randomNaturePlacement = false;
    [Range(0, 1)]
    public float randomNaturePlacementThreshold = 0.3f;
    public Dictionary<Vector3Int, GameObject> structuresDictionary = new Dictionary<Vector3Int, GameObject>();
    public Dictionary<Vector3Int, GameObject> natureDictionary = new Dictionary<Vector3Int, GameObject>();
    public float animationTime = 0.01f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator PlaceStructuresAroundRoad(List<Vector3Int> roadPositions)
    {
        Dictionary<Vector3Int, Direction> freeEstateSpots = FindFreeSpacesAroundRoad(roadPositions);
        List<Vector3Int> blockedPositions = new List<Vector3Int>();

        foreach (var freeSpot in freeEstateSpots)
        {
            if (blockedPositions.Contains(freeSpot.Key))
            {
                continue;
            }

            var rotation = Quaternion.identity;

            switch (freeSpot.Value)
            {
                case Direction.Up:
                    break;
                case Direction.Down:
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case Direction.Left:
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case Direction.Right:
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
                default:
                    break;
            }

            for (int i = 0; i < buildingTypes.Length; i++)
            {
                var buildingType = buildingTypes[i];

                if (buildingType.quantity == -1)
                {
                    if (randomNaturePlacement)
                    {
                        var random = UnityEngine.Random.value;

                        if (random < randomNaturePlacementThreshold)
                        {
                            var nature = SpawnPrefab(naturePrefabs[UnityEngine.Random.Range(0, naturePrefabs.Length)], freeSpot.Key, rotation);
                            natureDictionary.Add(freeSpot.Key, nature);
                            break;
                        }
                    }

                    var building = SpawnPrefab(buildingType.GetPrefab(), freeSpot.Key, rotation);
                    structuresDictionary.Add(freeSpot.Key, building);
                    break;
                }

                if (buildingType.IsBuildingAvailable())
                {
                    if (buildingType.sizeRequired > 1)
                    {
                        var halfSize = Mathf.FloorToInt(buildingType.sizeRequired / 2.0f);
                        List<Vector3Int> tempPositionsBlocked = new List<Vector3Int>();

                        if (VerifyIfBuildingFits(halfSize, freeEstateSpots, freeSpot, blockedPositions, ref tempPositionsBlocked))
                        {
                            blockedPositions.AddRange(tempPositionsBlocked);

                            var building = SpawnPrefab(buildingType.GetPrefab(), freeSpot.Key, rotation);
                            structuresDictionary.Add(freeSpot.Key, building);

                            foreach (var pos in tempPositionsBlocked)
                            {
                                structuresDictionary.Add(pos, building);
                            }
                        }
                    }
                    else
                    {
                        var building = SpawnPrefab(buildingType.GetPrefab(), freeSpot.Key, rotation);
                        structuresDictionary.Add(freeSpot.Key, building);
                    }

                    break;
                }
            }
            yield return new WaitForSeconds(animationTime);
        }
    }

    private bool VerifyIfBuildingFits(
        int halfSize,
        Dictionary<Vector3Int, Direction> freeEstateSpots,
        KeyValuePair<Vector3Int, Direction> freeSpot,
        List<Vector3Int> blockedPositions,
        ref List<Vector3Int> tempPositionsBlocked)
    {
        Vector3Int direction = Vector3Int.zero;

        if (freeSpot.Value == Direction.Up || freeSpot.Value == Direction.Down)
        {
            direction = Vector3Int.right;
        }
        else
        {
            direction = Vector3Int.forward;
        }

        for (int i = 1; i <= halfSize; i++)
        {
            var pos1 = freeSpot.Key + direction * i;
            var pos2 = freeSpot.Key - direction * i;

            if (!freeEstateSpots.ContainsKey(pos1) || !freeEstateSpots.ContainsKey(pos2)
                || blockedPositions.Contains(pos1) || blockedPositions.Contains(pos2))
            {
                return false;
            }

            tempPositionsBlocked.Add(pos1);
            tempPositionsBlocked.Add(pos2);
        }

        return true;
    }

    private GameObject SpawnPrefab(GameObject prefab, Vector3Int position, Quaternion rotation)
    {
        var newStructure = Instantiate(prefab, position, rotation, transform);
        newStructure.AddComponent<FallTween>();
        return newStructure;
    }

    private Dictionary<Vector3Int, Direction> FindFreeSpacesAroundRoad(List<Vector3Int> roadPositions)
    {
        Dictionary<Vector3Int, Direction> freeSpaces = new Dictionary<Vector3Int, Direction>();

        foreach (var position in roadPositions)
        {
            var neighbourDirections = PlacementHelper.FindNeighbour(position, roadPositions);

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (!neighbourDirections.Contains(direction))
                {
                    var newPosition = position + PlacementHelper.GetOffsetFromDirection(direction);

                    if (freeSpaces.ContainsKey(newPosition))
                    {
                        continue;
                    }

                    freeSpaces.Add(newPosition, PlacementHelper.GetReverseDirection(direction));
                }
            }
        }

        return freeSpaces;
    }

    public void Reset()
    {
        foreach (var item in structuresDictionary.Values)
        {
            Destroy(item);
        }
        structuresDictionary.Clear();

        foreach (var item in natureDictionary.Values)
        {
            Destroy(item);
        }
        natureDictionary.Clear();

        foreach (var buildingType in buildingTypes)
        {
            buildingType.Reset();
        }
    }
}
