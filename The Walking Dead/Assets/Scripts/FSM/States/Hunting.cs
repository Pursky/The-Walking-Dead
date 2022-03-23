using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunting : State
{
    public static float AttackingDistance = 1.5f;
    public static float AttackInterval = 1.4f;
    public static float LoseTime = 10;

    private Zombie zombie;
    private float attackTimer;
    private float outOfSightTimer;

    public Hunting(StateMachine stateMachine) : base(stateMachine)
    {
        zombie = (Zombie)stateMachine.Parent;
        zombie.Agent.speed = zombie.OriginalSpeed * 1.5f;
    }

    public override void TransitionCheck()
    {
        outOfSightTimer += Time.deltaTime;

        if (zombie.CanSensePlayer()) outOfSightTimer = 0; 

        if (Player.Instance.Dead || outOfSightTimer > LoseTime)
        {
            if (zombie.path) stateMachine.CurrentState = new Pathing(stateMachine);
            else stateMachine.CurrentState = new Roaming(stateMachine);
        }
    }

    public override void Update()
    {
        zombie.Agent.destination = Player.Instance.transform.position;

        attackTimer = attackTimer > 0 ? attackTimer -= Time.deltaTime : 0;

        if (Vector3.Distance(Player.Instance.transform.position, zombie.transform.position) < AttackingDistance && attackTimer <= 0)
        {
            zombie.StartAttacking();
            attackTimer = AttackInterval;
        }
    }
}