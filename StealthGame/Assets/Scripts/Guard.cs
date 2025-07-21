// Required: A* Pathfinding Project must be imported (Aron Granberg's A* Pathfinding)
// Attach this script to a guard GameObject with a Seeker + AIPath component

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding; // A* namespace

public class Guard : MonoBehaviour
{
    public float fov = 90f;
    public int rayCount = 30;
    public float distance = 5f;
    public LayerMask visionMask;
    public float maxSearchWait = 2f;

    private bool playerVisible = false;
    private bool playerWasVisibleLastFrame = false;
    public Vector2 lastSeenPosition;
    private bool hasReachedLastSeenPosition = false;
    private Vector2 interruptionPoint;

    private Waypoints wPoints;
    private int wayPointIndex;
    private Stack<Vector2> tempWayPoints = new Stack<Vector2>();

    private AIPath aiPath;
    private Seeker seeker;

    private enum GuardState { Patrolling, Chasing, Searching, Returning }
    private GuardState currentState = GuardState.Patrolling;

    private float searchWaitTimer = 0f;

    void Start()
    {
        Physics2D.queriesStartInColliders = false;
        wPoints = GameObject.FindGameObjectWithTag("Waypoints").GetComponent<Waypoints>();

        aiPath = GetComponent<AIPath>();
        seeker = GetComponent<Seeker>();
        aiPath.canSearch = true;
        aiPath.canMove = true;
        aiPath.maxSpeed = 3f; // Adjust as needed
    }

    void Update()
    {
        Vision();
        Debug.Log($"Current State: {currentState}");

        switch (currentState)
        {
            case GuardState.Patrolling:
                Patrol(); break;
            case GuardState.Chasing:
                Chase(); break;
            case GuardState.Searching:
                Search(); break;
            case GuardState.Returning:
                ReturnToPatrol(); break;
        }
    }

    void Vision()
    {
        playerWasVisibleLastFrame = playerVisible;
        playerVisible = false;

        float angleStep = fov / (rayCount - 1);
        float startAngle = -fov / 2;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * transform.up;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, visionMask);
            if (hit.collider != null)
            {
                Debug.DrawLine(transform.position, hit.point, hit.collider.CompareTag("Player") ? Color.red : Color.yellow);

                if (hit.collider.CompareTag("Player"))
                {
                    playerVisible = true;
                    lastSeenPosition = hit.point;
                }
            }
            else
            {
                Debug.DrawLine(transform.position, (Vector2)transform.position + direction * distance, Color.green);
            }
        }

        if (playerVisible && !playerWasVisibleLastFrame)
        {
            Debug.Log("Player spotted!");
            if (currentState == GuardState.Patrolling)
            {
                interruptionPoint = transform.position;
                tempWayPoints.Clear();
                tempWayPoints.Push(interruptionPoint);
            }
            currentState = GuardState.Chasing;
        }
    }

    void Patrol()
    {
        if (wPoints == null || wPoints.waypoints.Length == 0) return;

        aiPath.destination = wPoints.waypoints[wayPointIndex].position;

        if (!aiPath.pathPending && aiPath.reachedDestination)
        {
            wayPointIndex = (wayPointIndex + 1) % wPoints.waypoints.Length;
        }
    }

    void Chase()
    {
        aiPath.destination = lastSeenPosition;

        // Optional fallback check: manually confirm if close enough
        float distanceToTarget = Vector2.Distance(transform.position, lastSeenPosition);
        if (distanceToTarget < 0.2f || (!aiPath.pathPending && aiPath.reachedDestination))
        {
            hasReachedLastSeenPosition = true;
        }

        if (!playerVisible && hasReachedLastSeenPosition)
        {
            Debug.Log("Reached last seen position, starting to search...");
            searchWaitTimer = maxSearchWait;
            tempWayPoints.Push(lastSeenPosition);
            currentState = GuardState.Searching;
            hasReachedLastSeenPosition = false;
        }

        // Reset if the player becomes visible again mid-chase
        if (playerVisible)
        {
            hasReachedLastSeenPosition = false;
        }
    }

    void Search()
    {
        searchWaitTimer -= Time.deltaTime;
        if (searchWaitTimer <= 0f)
        {
            currentState = tempWayPoints.Count > 0 ? GuardState.Returning : GuardState.Patrolling;
        }
    }

    void ReturnToPatrol()
    {
        if (tempWayPoints.Count == 0)
        {
            currentState = GuardState.Patrolling;
            return;
        }

        Vector2 target = tempWayPoints.Peek();
        aiPath.destination = target;

        if (!aiPath.pathPending && aiPath.reachedDestination)
        {
            tempWayPoints.Pop();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastSeenPosition, 0.15f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(interruptionPoint, 0.15f);

        Gizmos.color = Color.magenta;
        foreach (var point in tempWayPoints)
        {
            Gizmos.DrawWireCube(point, Vector3.one * 0.1f);
        }

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
    }
}
