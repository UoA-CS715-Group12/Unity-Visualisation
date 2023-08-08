using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SimpleVisualizer;

public class Visualizer : MonoBehaviour
{
    public LSystemGenerator lsystem;
    public RoadHelper roadHelper;
    public StructureHelper structureHelper;
    public int roadLength = 8;

    private int length = 8;
    private float angle = 90f;
    private bool waitingForTheRoad = false;

    public int Length
    {
        get
        {
            if (length > 0)
            {
                return length;
            }
            else
            {
                return 1;
            }
        }
        set => length = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        roadHelper.finishedCoroutine += () =>
        {
            waitingForTheRoad = false;
        };
        CreateTown();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateTown()
    {
        length = roadLength;
        roadHelper.Reset();
        structureHelper.Reset();
        var sequence = lsystem.GenerateSentence();
        StartCoroutine(VisualizeSequence(sequence));
    }

    private IEnumerator VisualizeSequence(string sequence)
    {
        Stack<AgentParameters> savePoints = new Stack<AgentParameters>();
        var currentPosition = Vector3.zero;

        Vector3 direction = Vector3.forward;
        Vector3 tempPosition = Vector3.zero;

        foreach (var letter in sequence)
        {
            if (waitingForTheRoad)
            {
                yield return new WaitForEndOfFrame();
            }

            EncodingLetters encoding = (EncodingLetters)letter;

            switch (encoding)
            {
                case EncodingLetters.save:
                    savePoints.Push(new AgentParameters()
                    {
                        position = currentPosition,
                        direction = direction,
                        length = Length
                    });
                    break;

                case EncodingLetters.load:
                    if (savePoints.Count > 0)
                    {
                        var agentParameters = savePoints.Pop();
                        currentPosition = agentParameters.position;
                        direction = agentParameters.direction;
                        Length = agentParameters.length;
                    }
                    else
                    {
                        throw new System.Exception("Don't have saved point in our stack");
                    }
                    break;

                case EncodingLetters.draw:
                    tempPosition = currentPosition;
                    currentPosition += direction * length;
                    StartCoroutine(roadHelper.PlaceStreetPositions(tempPosition, Vector3Int.RoundToInt(direction), length));
                    waitingForTheRoad = true;

                    yield return new WaitForEndOfFrame();

                    Length -= 2;
                    break;

                case EncodingLetters.turnRight:
                    direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                    break;

                case EncodingLetters.turnLeft:
                    direction = Quaternion.AngleAxis(-angle, Vector3.up) * direction;
                    break;

                default:
                    break;
            }
        }

        yield return new WaitForSeconds(0.1f);
        roadHelper.FixRoad();
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(structureHelper.PlaceStructuresAroundRoad(roadHelper.GetRoadPositions()));
    }
}
