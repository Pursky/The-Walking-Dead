using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing : State
{
    public static float PointTolerance = 2;
    public static float RandomRadius = 1.5f;

    private Zombie zombie;
    private int pathIndex;
    private Vector3 nextPoint;

    public Pathing(StateMachine stateMachine) : base(stateMachine)
    {
        zombie = (Zombie)stateMachine.Parent;
        pathIndex = GetClosestPointIndex();
        nextPoint = GetNextPoint();
        zombie.Agent.speed = zombie.OriginalSpeed;
    }

    public override void TransitionCheck()
    {
        if (zombie.CanSensePlayer() && !Player.Instance.Dead)
        {
            stateMachine.CurrentState = new Hunting(stateMachine);
        }
    }

    public override void Update()
    {
        zombie.Agent.destination = nextPoint;

        if (Vector3.Distance(zombie.transform.position, nextPoint) < PointTolerance)
        {
            pathIndex++;
            if (pathIndex == zombie.path.Points.Length) pathIndex = 0;
            nextPoint = GetNextPoint();
        }
    }

    private Vector3 GetNextPoint()
    {
        Vector2 random = Random.insideUnitCircle * Random.Range(0, RandomRadius);
        return zombie.path.Points[pathIndex] + new Vector3(random.x, 0, random.y);
    }

    private int GetClosestPointIndex()
    {
        int closestPointIndex = 0;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < zombie.path.Points.Length; i++)
        {
            float distance = Vector3.Distance(zombie.transform.position, zombie.path.Points[i]);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestPointIndex = i;
            }
        }

        return closestPointIndex;
    }
}