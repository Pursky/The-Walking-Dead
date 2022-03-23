using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    [Header("Textures")]
    public Texture[] IdleTextures;
    public TextureContainer[] AnimTextures;
    public Texture[] DieTextures;
    public Texture[] AttackTextures;
    
    [Header("Material")]
    public Material ZombiePresetMaterial;

    [Header("Prefab")]
    public GameObject HitPrefab;

    [Header("Path")]
    public Path path;

    [Header("Zombie Values")]
    public float Strength;
    public float SpottingDistance;
    public float SpottingAngle;
    public float RaycastCooldownTime;
    public float HearingDistance;
    public float FeelingDistance;
    public float GroanInterval;
    public float RandomGroanTimeOffset;

    [Header("Intervals")]
    public float WalkAnimationInterval;
    public float DieAnimationInterval;
    public float AttackAnimationInterval;

    [Header("Health")]
    [Range(0, 10)] public float Health;

    [Header("Audio")]
    public AudioClip[] GroanClips;

    [HideInInspector] public NavMeshAgent Agent;
    [HideInInspector] public float OriginalSpeed;

    private Material material;
    private MeshRenderer body;
    private StateMachine stateMachine;
    private AudioSource scuff;
    private AudioSource audioSource;

    private float animationTimer;
    private int animationIndex;

    private bool attacking;
    private bool dying;

    private float raycastCooldown;
    private float currentWalkAnimationInterval;

    private float groanTimer;
    private int lastGroanIndex;

    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        material = new Material(ZombiePresetMaterial);
        OriginalSpeed = Agent.speed;

        scuff = GetComponent<AudioSource>();

        scuff.time = Random.Range(0, scuff.clip.length);
        audioSource = GetComponents<AudioSource>()[1];

        body = transform.GetChild(0).GetComponent<MeshRenderer>();
        body.material = material;

        stateMachine = new StateMachine(this);

        if (path) stateMachine.SetInitState(new Pathing(stateMachine));
        else stateMachine.SetInitState(new Roaming(stateMachine));

        animationIndex = UnityEngine.Random.Range(0, 4);
        animationTimer = UnityEngine.Random.Range(Time.time, Time.time + WalkAnimationInterval);

        Agent.enabled = true;
    }

    void Update()
    {
        if (UI.Instance.Paused) return;

        if (!dying) stateMachine.Update();

        if (dying) HandleDying();
        else HandleSprites();

        HandleGroaning();

        if (Agent.enabled)
        {
            scuff.pitch = Agent.speed;

            if (Agent.velocity.magnitude > Agent.speed / 5f) scuff.volume = 1;
            else scuff.volume = 0;
        }
        else scuff.volume = 0;

        raycastCooldown = raycastCooldown >= 0 ? raycastCooldown - Time.deltaTime : 0;
    }

    private void HandleSprites()
    {
        if (attacking)
        {
            HandleAttacking();
            return;
        }

        int textureIndex = GetTextureIndex();
        material.mainTexture = IdleTextures[textureIndex];

        if (Agent.velocity.magnitude == 0) return;
        material.mainTexture = AnimTextures[textureIndex].Textures[animationIndex];

        currentWalkAnimationInterval = WalkAnimationInterval * (OriginalSpeed / Agent.speed);

        if (Time.time > animationTimer)
        {
            animationTimer = Time.time + currentWalkAnimationInterval;
            animationIndex = animationIndex < 3 ? animationIndex + 1 : 0;
        }
    }

    private void HandleAttacking()
    {
        if (Time.time > animationTimer)
        {
            animationTimer = Time.time + AttackAnimationInterval;
            if (animationIndex < AttackTextures.Length - 1) animationIndex++;

            material.mainTexture = AttackTextures[animationIndex];

            if (animationIndex == AttackTextures.Length - 3) if (CanHitPlayer()) Player.Instance.TakeDamage(Strength);

            if (animationIndex == AttackTextures.Length - 1)
            {
                attacking = false;
                Agent.isStopped = false;
                animationIndex = 0;
                animationTimer = 0;
            }
        }
    }

    private int GetTextureIndex()
    {
        Vector3 distance = (Player.Instance.transform.position - transform.position);
        distance.y = 0;

        float angle = Vector3.Angle(transform.forward, distance);
        if (Vector3.Angle(transform.right, distance) < 90) angle = -angle + 360;

        int index = (int)(angle / 45f + 0.5f);
        if (index == 8) index = 0;
            
        return index;
    }

    private void HandleGroaning()
    {
        if (dying) return;

        if (Time.time > groanTimer)
        {
            groanTimer = Time.time + GroanInterval + Random.Range(-RandomGroanTimeOffset, RandomGroanTimeOffset);

            int index;
            do index = Random.Range(0, GroanClips.Length);
            while (index == lastGroanIndex);

            lastGroanIndex = index;
            audioSource.PlayOneShot(GroanClips[index]);
        }
    }

    public void TakeDamage(bool headshot, Vector3 hitPosition)
    {
        if (Health == 0) return;

        if (headshot || Player.Instance.GodMode) Health -= 10;
        else Health -= 4;

        Transform hit = Instantiate(HitPrefab, hitPosition, Quaternion.identity).transform;
        hit.forward = body.transform.up;

        Health = Mathf.Clamp(Health, 0, 10);

        if (Health == 0)
        {
            Gun.Instance.ChangeCrosshair();

            body.GetComponents<Collider>()[0].enabled = false;
            body.GetComponents<Collider>()[1].enabled = false;
            body.transform.GetChild(0).GetComponent<Collider>().enabled = false;

            dying = true;
            Agent.enabled = false;
            animationTimer = 0;
            animationIndex = 0;
        }
    }

    private void HandleDying()
    {
        if (Time.time > animationTimer)
        {
            animationTimer = Time.time + DieAnimationInterval;
            if (animationIndex < DieTextures.Length - 1) animationIndex++;

            material.mainTexture = DieTextures[animationIndex];

            if (animationIndex == DieTextures.Length - 1)
            {
                Invoke("Delete", 4);
            }
        }

        if (animationIndex == DieTextures.Length - 1) transform.position += Vector3.down * Time.deltaTime * 0.2f;
    }

    public void StartAttacking()
    {
        if (EndingTrigger.GameOver) return;

        transform.forward = Player.Instance.transform.position - transform.position;
        Agent.isStopped = true;
        attacking = true;
        animationTimer = 0;
        animationIndex = 0;
    }

    private bool CanSeePlayer()
    {
        Vector3 headPos = transform.position + Vector3.up * 0.5f;

        Vector3 distance = (Player.Instance.transform.position - headPos);
        if (distance.magnitude > SpottingDistance) return false;

        float angle = Vector3.Angle(transform.forward, distance);

        if (angle < SpottingAngle && raycastCooldown == 0)
        {
            raycastCooldown = RaycastCooldownTime;

            Physics.Raycast(new Ray(headPos, distance), out RaycastHit hit);
            return hit.collider.gameObject.layer == 11;
        }

        return false;
    }

    private bool CanHearPlayer()
    {
        float distance = Vector3.Distance(Player.Instance.transform.position, transform.position);
        return Gun.Instance.Shooting && distance < HearingDistance;
    }

    private bool CanFeelPlayer()
    {
        float distance = Vector3.Distance(Player.Instance.transform.position, transform.position);
        return distance < FeelingDistance;
    }

    public bool CanSensePlayer() => CanSeePlayer() || CanHearPlayer() || CanFeelPlayer();
 
    private void Delete()
    {
        Destroy(gameObject);
    }

    public bool CanHitPlayer()
    {
        Vector3 distance = Player.Instance.transform.position - transform.position;
        if (distance.magnitude > Hunting.AttackingDistance + 0.5f) return false;

        Physics.Raycast(new Ray(transform.position, distance), out RaycastHit hit);
        return hit.collider.gameObject.layer == 11;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        Vector3 headPos = transform.position + Vector3.up * 0.5f;

        Vector2 relativeVector = new Vector2(CameraController.DegSin(SpottingAngle), CameraController.DegCos(SpottingAngle));
        relativeVector *= SpottingDistance;

        Vector3[] points = new Vector3[]
        {
            headPos + transform.forward * relativeVector.y + transform.up * relativeVector.x,
            headPos + transform.forward * relativeVector.y + transform.right * relativeVector.x,
            headPos + transform.forward * relativeVector.y - transform.up * relativeVector.x,
            headPos + transform.forward * relativeVector.y - transform.right * relativeVector.x
        };

        Vector3 tip = headPos + transform.forward * SpottingDistance;

        foreach (Vector3 point in points)
        {
            Gizmos.DrawLine(headPos, point);
            Gizmos.DrawLine(tip, point);
        }

        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[1], points[2]);
        Gizmos.DrawLine(points[2], points[3]);
        Gizmos.DrawLine(points[3], points[0]);
    }
}