using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AirplaneController : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Vector3> pathPoints = new List<Vector3>();
    private bool isDrawing = false;
    private float speed = 2.5f;
    private int currentPathIndex;

    private bool isMovingToCenter = true;

    private Vector3 moveDirection;
    private bool hasCalculatedDirection = false;



    void Start()
    {

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.widthMultiplier = 0.08f;
        lineRenderer.sortingLayerName = "Plane";
        lineRenderer.sortingOrder = 5;
        Material lineMaterial = new(Shader.Find("Unlit/Color"))
        {
            color = Color.white
        };
        lineRenderer.material = lineMaterial;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if (isMovingToCenter)
        {
            MoveToCenter();
        }

        // Handle touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            touchPosition.z = 0; // Ensure the touch position is in the same plane as the game objects

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Check if the touch is close enough to the plane to start drawing a path
                    if (Vector2.Distance(touchPosition, transform.position) < 1f) // Use an appropriate touch radius
                    {
                        StartPath(touchPosition);
                        isDrawing = true;
                        isMovingToCenter = false;
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    // If the user is drawing, add points to the path
                    if (isDrawing)
                    {
                        AddPathPoint(touchPosition);
                    }
                    break;

                case TouchPhase.Ended:
                    // User lifted the finger, stop drawing
                    isDrawing = false;
                    break;
            }
        }

        // Follow the path that has been drawn
        if (pathPoints.Count > 0)
        {
            FollowPath();
        }
    }
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed; // Update the speed
    }
    void MoveToCenter()
    {
        if (!hasCalculatedDirection)
        {
            Vector3 centerPosition = new Vector3(0, 0, 0);
            moveDirection = (centerPosition - transform.position).normalized;
        }

        // Move in the calculated direction
        transform.position += moveDirection * speed * Time.deltaTime;

        if (!hasCalculatedDirection)
        {
            RotateTowards(transform.position + moveDirection);
            hasCalculatedDirection = true;
        }
    }

    void StartPath(Vector3 position)
    {
        isMovingToCenter = false;
        position.z = -1;
        isDrawing = true;
        pathPoints.Clear();
        lineRenderer.positionCount = 0;
        AddPathPoint(position);
    }

    void AddPathPoint(Vector3 point)
    {
        point.z = -1;

        if (pathPoints.Count == 0 || Vector3.Distance(point, pathPoints[pathPoints.Count - 1]) > 0.1f) // Threshold to avoid too many points
        {
            pathPoints.Add(point);
            lineRenderer.positionCount = pathPoints.Count;
            lineRenderer.SetPositions(pathPoints.ToArray());
        }
    }

    void FollowPath()
    {
        if (currentPathIndex < pathPoints.Count)
        {
            // Move towards the next point in the path
            transform.position = Vector3.MoveTowards(transform.position, pathPoints[currentPathIndex], speed * Time.deltaTime);

            // Rotate the airplane to face the next point
            if (currentPathIndex < pathPoints.Count - 1)
            {
                RotateTowards(pathPoints[currentPathIndex + 1]);
            }

            // Check if the plane has reached the current path point
            if (transform.position == pathPoints[currentPathIndex])
            {
                // Move on to the next point
                currentPathIndex++;
            }
        }
        else
        {
            // Once the plane has completed the path
            Vector3 lastPoint = pathPoints[pathPoints.Count - 1];
            Vector3 secondLastPoint = pathPoints.Count > 1 ? pathPoints[pathPoints.Count - 2] : lastPoint - new Vector3(0, -1, 0); // Here I'm assuming a default direction

            // Calculate the direction to continue moving in.
            Vector3 extrapolatedDirection = (lastPoint - secondLastPoint).normalized;

            // You could set a distance you want the plane to travel after finishing the path.
            float extrapolatedDistance = 20.0f; // This is an arbitrary number; set it according to your needs.
            Vector3 extrapolatedTarget = lastPoint + extrapolatedDirection * extrapolatedDistance;

            // Keep moving towards the extrapolated target.
            transform.position = Vector3.MoveTowards(transform.position, extrapolatedTarget, speed * Time.deltaTime);

            // The plane should still rotate towards the extrapolated direction.
            RotateTowards(extrapolatedTarget);
            ClearPath();

        }
    }

    void ClearPath()
    {
        //pathPoints.Clear();
        lineRenderer.positionCount = 0;
    }

    void RotateTowards(Vector3 targetPoint)
    {
        if (isMovingToCenter)
        {
            Debug.Log("rotatetowards");
            // Directly face the center at the start by finding the direction to the center
            Vector3 directionToCenter = (new Vector3(0, 0, 0) - transform.position).normalized;

            // Calculate the angle from the airplane to the center
            float angleToCenter = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;

            // Since the airplanes might be oriented in a way where their 'forward' is not their actual forward, 
            // we adjust the angle by 180 degrees if necessary
            angleToCenter += 90f;

            // Set the rotation to face the center
            transform.rotation = Quaternion.AngleAxis(angleToCenter, Vector3.forward);

        }
        else
        {
            Debug.Log("rotatetowards");
            // Rotate towards the given target point
            Vector3 directionToTarget = (targetPoint - transform.position) * -1;
            directionToTarget.z = 0; // Ensure we only rotate on the y-axis, as this is a 2D game
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget.normalized);

            // Here 'rotationSpeed' is a float that determines how quickly the airplane turns
            float rotationSpeed = 150f;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Plane") || other.CompareTag("Border") || other.CompareTag("Gplane") || other.CompareTag("Helicopter"))
        {
            EndGame();
        }

        if (other.CompareTag("Plane") || other.CompareTag("Gplane") || other.CompareTag("Helicopter"))
        {
            // Logic when a plane collides with another plane
            Destroy(gameObject); // Destroy this plane
            Destroy(other.gameObject); // Destroy the other plane
        }

        if ((gameObject.tag == "Plane" && (other.tag == "Gland" || other.tag == "Rland")))
        {
            Vector3 touchdownPoint = other.bounds.center;

            // Disable the plane's normal controls
            DisablePlaneControls();

            // Start the landing animation
            StartCoroutine(AnimateLanding(transform, other.transform));
            ScoreManager.instance.AddPoint();
        }

        if ((gameObject.tag == "Gplane" && other.tag == "Gland"))
        {
            Vector3 touchdownPoint = other.bounds.center;

            // Disable the plane's normal controls
            DisablePlaneControls();

            // Start the landing animation
            StartCoroutine(AnimateLanding(transform, other.transform));
            ScoreManager.instance.AddPoint();
        }

        if (gameObject.CompareTag("Helicopter") && other.CompareTag("Hland"))
        {
            // Stop the helicopter from moving and responding to further collisions
            gameObject.GetComponent<AirplaneController>().enabled = false;
            gameObject.GetComponent<Collider2D>().enabled = false;

            // Start the landing animation
            StartCoroutine(AnimateLanding(transform, other.transform));
            ScoreManager.instance.AddPoint();
        }

        if ((gameObject.tag == "Gplane" && other.tag == "Gland") ||
            (gameObject.tag == "Plane" && (other.tag == "Gland" || other.tag == "Rland")) ||
            (gameObject.tag == "Helicopter" && other.tag == "Hland"))
        {
            // Stop the plane by clearing its path
            ClearPath();

            // Additional logic to handle the plane landing, e.g., play animations, sound, etc.
            HandlePlaneLanding();
        }
    }

    void DisablePlaneControls()
    {
        // Disable the script that controls the plane's movement
        this.enabled = false;

        // Disable the plane's collider
        GetComponent<Collider2D>().enabled = false;
    }

    void HandlePlaneLanding()
    {
        // Implement the landing logic here
        // For now, let's just log to the console

        // Here you can set isLanded to true if you have such a variable,
        // and then in the Update method, only move the plane if it's not landed
    }

    IEnumerator AnimateLanding(Transform helicopter, Transform landingSpot)
    {
        // Define the duration of the landing and spinning animation
        float duration = 2.5f;
        float elapsed = 0.0f;

        // Get the initial scale and rotation of the helicopter
        Vector3 originalScale = helicopter.localScale;
        Quaternion originalRotation = helicopter.rotation;

        // Define how much the helicopter should shrink
        Vector3 targetScale = Vector3.zero; // Shrink to nothing

        // Define the speed of the rotation (degrees per second)
        float rotationSpeed = 360.0f; // One full rotation per second

        // Calculate the center of the landing spot
        Vector3 centerOfLandingSpot = landingSpot.position; // Adjust as needed based on the collider's offset

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fraction = elapsed / duration;

            // Move towards the center of the landing spot
            helicopter.position = Vector3.Lerp(helicopter.position, centerOfLandingSpot, fraction);

            // Spin the helicopter
            helicopter.rotation = originalRotation * Quaternion.Euler(0, 0, rotationSpeed * elapsed);

            // Shrink the helicopter
            helicopter.localScale = Vector3.Lerp(originalScale, targetScale, fraction);

            yield return null;
        }

        // Optionally deactivate the helicopter if you don't want it destroyed
        helicopter.gameObject.SetActive(false);
        // Or destroy the helicopter if you don't need it anymore
        // Destroy(helicopter.gameObject);
    }

    IEnumerator AnimatePlaneLanding(Transform plane, Vector3 touchdownPoint)
    {
        float descentRate = 0.1f; // The rate at which the plane descends to the runway
        float decelerationRate = 0.95f; // The rate at which the plane decelerates
        float minimumSpeed = 0.1f; // The minimum speed before the plane stops

        // Assuming the plane's forward direction is the direction of landing
        while (plane.position.y > touchdownPoint.y)
        {
            // Simulate the descent by gradually lowering the y position of the plane until it reaches the runway
            plane.position = new Vector3(plane.position.x, plane.position.y - descentRate * Time.deltaTime, plane.position.z);

            // Wait for the next frame
            yield return null;
        }

        // Now the plane is on the runway and should decelerate to a stop
        Vector3 startPosition = plane.position;
        float speed = (touchdownPoint - startPosition).magnitude / 3.0f; // Initial speed

        while (speed > minimumSpeed)
        {
            // Decelerate the plane's speed
            speed *= decelerationRate;

            // Move the plane forward along the runway
            plane.position += plane.forward * speed * Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Optionally add a bit of roll-out distance here
        // ...

        // The plane has now stopped, so you can trigger any post-landing events
        plane.gameObject.SetActive(false); // Or whatever you want to do with the plane
    }

    internal void SetDirection(Vector2 direction)
    {
        throw new NotImplementedException();
    }

    public void EndGame()
    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
