using System.Collections;
using UnityEngine;

public class FallTween : MonoBehaviour
{
    private Vector3 destination;
    public float timeToFall = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        destination = transform.position;
        gameObject.transform.position += Vector3.up * 10;
        StartCoroutine(Drop());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator Drop()
    {
        Vector3 position = gameObject.transform.position;
        float currentTime = 0f;

        do
        {
            gameObject.transform.position = Vector3.Lerp(position, destination, currentTime / timeToFall);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= timeToFall);

        gameObject.transform.position = destination;
    }
}
