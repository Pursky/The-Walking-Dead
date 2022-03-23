using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [Header("Player Values")]
    public float MovementSpeed;
    public float JumpForce;
    public float DamageCoolDown;
    public float MinFallDamageTime;
    public float FallDamageModifier;
    public bool GodMode;

    [Header("Health")]
    [Range(0, 10)] public float Health;

    [Header("Audio")]
    public AudioClip[] WoodFootstepClips;
    public AudioClip[] RockFootstepClips;
    public AudioClip DamageClip;

    [HideInInspector] public Rigidbody Rigbod;
    [HideInInspector] public bool Grounded;
    [HideInInspector] public bool Dead;

    private enum GroundType {Wood, Rock}
    private AudioSource audioSource;
    private AudioSource audioSourceOverdrive;
    private Vector3 startPosition;
    private float damageCoolDownTimer;
    private float xMove;
    private float yMove;
    private float fallTimer;

    private float footStepTimer;
    private int lastFootstepIndex;
    private GroundType groundType;

    void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this);

        startPosition = transform.position;
        Rigbod = GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();
        audioSourceOverdrive = GetComponents<AudioSource>()[1];

        
    }

    void Update()
    {
        if (UI.Instance.Paused) return;

        if (Dead)
        {
            Rigbod.velocity = new Vector3(0, Rigbod.velocity.y, 0);
            return;
        }

        xMove = Input.GetAxis("Horizontal");
        yMove = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump") && Grounded) Jump();

        fallTimer =  Rigbod.velocity.y < 0 ? fallTimer + Time.deltaTime : 0;

        damageCoolDownTimer = damageCoolDownTimer >= 0 ? damageCoolDownTimer - Time.deltaTime : 0;

        HandleFootsteps();

        if (transform.position.y < -100)
        {
            Rigbod.velocity = Vector3.zero;
            fallTimer = 0;
            transform.position = startPosition;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && GodMode) transform.position = new Vector3(0, 1, 0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && GodMode) transform.position = new Vector3(-48, -7, 1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && GodMode) transform.position = new Vector3(-94, -14, 1);
    }

    private void FixedUpdate()
    {
        if (Dead) return;

        Vector3 movement = (transform.forward * yMove + transform.right * xMove);
        if (movement.magnitude > 1) movement.Normalize();
        movement *= MovementSpeed;

        Rigbod.velocity = new Vector3(movement.x, Rigbod.velocity.y, movement.z);
    }

    private void Jump()
    {
        Rigbod.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
    }

    public void TakeDamage(float damage)
    {
        if (damageCoolDownTimer > 0 || Dead || GodMode) return;

        audioSourceOverdrive.PlayOneShot(DamageClip);

        CameraController.Instance.AddKnockback();
        UI.Instance.DamageCover.color = new Color(0.25f, 0, 0, 0.5f);

        Health -= damage;
        Health = Mathf.Clamp(Health, 0, 10);

        UI.Instance.UpdateHealthBar();

        damageCoolDownTimer = DamageCoolDown;

        if (Health == 0)
        {
            Dead = true;

            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            collider.height = 1f;
            collider.center = new Vector3(0, 0.4f, 0);

            UI.Instance.LoadedText.transform.parent.gameObject.SetActive(false);
            UI.Instance.Crosshair.gameObject.SetActive(false);
            UI.Instance.Invoke("Restart", 3);
        }
    }
    
    private void HandleFootsteps()
    {
        if (Rigbod.velocity.magnitude < MovementSpeed / 5f || !Grounded || Dead) return;

        if (Time.time > footStepTimer)
        {
            footStepTimer = Time.time + CameraController.Instance.ViewBobbingTime * 0.5f;

            int index;
            do index = Random.Range(0, 4);
            while (index == lastFootstepIndex);

            lastFootstepIndex = index;

            if (groundType == GroundType.Rock) audioSource.PlayOneShot(RockFootstepClips[index]);
            else audioSource.PlayOneShot(WoodFootstepClips[index]);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (fallTimer > MinFallDamageTime) TakeDamage((fallTimer - MinFallDamageTime) * FallDamageModifier);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 2) return;

        if (other.gameObject.layer == 12) groundType = GroundType.Rock;
        else groundType = GroundType.Wood;

        Grounded = true;
        fallTimer = 0;
    }

    private void OnTriggerExit(Collider other)
    {
        Grounded = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, new Vector3(1,2,1));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * 0.5f + transform.forward);
    }
}