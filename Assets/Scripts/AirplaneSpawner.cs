using UnityEngine;

public class AirplaneSpawner : MonoBehaviour
{
    public GameObject gplanePrefab;
    public GameObject planePrefab;
    public GameObject helicopterPrefab;
    public float initialSpawnInterval = 10.0f; // Initial time interval between spawns
    public float speedIncreaseFactor = 10.0f; // Factor by which speed increases
    public int scoreThreshold = 10; // Score threshold to increase difficulty

    private float spawnInterval; // Current spawn interval
    private float speed; // Current speed
    private int lastScoreThreshold; // Last score at which difficulty was increased
    private float timer;

    void Start()
    {
        spawnInterval = initialSpawnInterval;
        speed = 1.5f; // Set your initial plane speed
        lastScoreThreshold = 0; // Initialize score threshold
        SpawnRandomAirplane();
    }

    void Update()
    {
        if (ScoreManager.instance != null && ScoreManager.instance.Score >= lastScoreThreshold + scoreThreshold)
        {
            lastScoreThreshold += scoreThreshold; // Update the last score threshold
            spawnInterval *= 0.9f; // Decrease interval to increase frequency
            speed *= speedIncreaseFactor; // Increase speed of planes reasonably

            // Debug to check new values
            Debug.Log("New spawn interval: " + spawnInterval + ", New plane speed: " + speed);
        }

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnRandomAirplane();
            timer = 0f;
        }
    }

    void SpawnRandomAirplane()
    {
        Vector3 worldCenter = new Vector3(0, 0, 0);
        Vector3 spawnPosition = GetRandomEdgePosition();
        Quaternion spawnRotation = Quaternion.LookRotation(Vector3.forward, worldCenter - spawnPosition);

        // Select a random airplane prefab
        GameObject[] prefabs = new GameObject[] { gplanePrefab, planePrefab, helicopterPrefab };
        GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)];

        // Instantiate it at the spawn position, looking towards the center
        GameObject newPlane = Instantiate(selectedPrefab, spawnPosition, spawnRotation);
        AirplaneController airplaneController = newPlane.GetComponent<AirplaneController>();
        newPlane.GetComponent<AirplaneController>().SetSpeed(speed);

        // After instantiation, you may want to set the airplane moving towards the center
        // This can be handled by a script attached to the airplane prefab
    }


    Vector3 GetRandomEdgePosition()
    {
        // Return a random position along the edge of the screen
        // This will depend on your camera setup, but here's a basic example for a screen edge position
        float randomX = Random.value > 0.5f ? 0 : Screen.width;
        float randomY = Random.Range(0, Screen.height);
        Vector3 screenPosition = new Vector3(randomX, randomY, 10); // 10 should be the distance from the camera to the GameObjects
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        return worldPosition;
    }
}
