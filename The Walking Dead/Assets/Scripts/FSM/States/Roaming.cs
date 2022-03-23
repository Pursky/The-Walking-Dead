using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Roaming : State
{
    public static float RoamDistance = 5;
    public static float WaitTime = 4;
    public static float WaitRandomOffset = 2f;

    private float waitTimer;
    private Zombie zombie;
    private Vector3 startPosition;
    private Vector3 nextPoint;

    public Roaming(StateMachine stateMachine) : base(stateMachine)
    {
        zombie = (Zombie)stateMachine.Parent;
        startPosition = zombie.transform.position;
        nextPoint = GetNextPoint();
        waitTimer = Time.time + Random.Range(0, WaitTime);
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

        if (Time.time > waitTimer)
        {
            nextPoint = GetNextPoint();
            waitTimer = GetWaitTime();
        }
    }

    private Vector3 GetNextPoint()
    {
        Vector2 random = Random.insideUnitCircle * RoamDistance;
        return startPosition + new Vector3(random.x, 0, random.y);
    }

    private float GetWaitTime() => Time.time + WaitTime + Random.Range(-WaitRandomOffset, WaitRandomOffset);
}