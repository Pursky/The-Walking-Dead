using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("Camera Values")]
    public float RotationSpeed;
    public float ViewBobbingStrength;
    public float ViewBobbingFade;
    public float KnockbackForce;

    [HideInInspector] public float ViewBobbingTime;

    private float viewBobbingTimer;
    private Vector3 relativePosition = Vector3.zero;
    private float currentViewBobbingStrength;

    private Vector3 knockback = Vector3.zero;

    void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this);

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (UI.Instance.Paused) return;

        transform.position = Player.Instance.transform.position + Vector3.up * 0.5f + relativePosition;

        if (Player.Instance.Dead) return;

        float xMove = Input.GetAxis("Mouse X");
        float yMove = Input.GetAxis("Mouse Y");

        Vector3 rotation = transform.eulerAngles;
        rotation += new Vector3(-yMove, xMove, 0) * RotationSpeed + new Vector3(-knockback.y, knockback.x) * Time.deltaTime;

        if (rotation.x > 180) rotation.x -= 360;
        rotation.x = Mathf.Clamp(rotation.x, -90, 90);

        rotation.z = 0;

        transform.eulerAngles = rotation;

        Player.Instance.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        knockback -= knockback * Time.deltaTime * 10;
        if (knockback.magnitude < 0.05f) knockback = Vector3.zero;

        HandleViewBobbing();
    }

    private void HandleViewBobbing()
    {
        ViewBobbingTime = -Player.Instance.MovementSpeed * 0.08f + 1.2f;

        float vertical = Mathf.Sqrt(Mathf.Pow(Input.GetAxis("Vertical"), 2));
        float horizontal = Mathf.Sqrt(Mathf.Pow(Input.GetAxis("Horizontal"), 2));

        if ((Mathf.Approximately(vertical, 1) || Mathf.Approximately(horizontal, 1)) && Player.Instance.Grounded)
        {
            if (currentViewBobbingStrength < ViewBobbingStrength) currentViewBobbingStrength += ViewBobbingStrength * Time.deltaTime * ViewBobbingFade;
            else currentViewBobbingStrength = ViewBobbingStrength;
        }
        else
        {
            if (currentViewBobbingStrength > 0) currentViewBobbingStrength -= ViewBobbingStrength * Time.deltaTime * ViewBobbingFade;
            else currentViewBobbingStrength = 0;
        }

        float hValue = Mathf.Sin((2 * Mathf.PI * viewBobbingTimer) / ViewBobbingTime);
        float vValue = Mathf.Cos((4 * Mathf.PI * viewBobbingTimer) / ViewBobbingTime) * 0.5f;

        Vector3 hWave = transform.right * hValue * currentViewBobbingStrength;
        Vector3 vWave = (transform.up * vValue + Vector3.up * 0.5f) * currentViewBobbingStrength;

        relativePosition = hWave + vWave;

        viewBobbingTimer += Time.deltaTime;
        if (viewBobbingTimer > ViewBobbingTime) viewBobbingTimer = 0;
    }

    public void AddKnockback()
    {
        float angle = Random.Range(-20, 20);
        knockback = new Vector3(DegSin(angle), DegCos(angle)) * KnockbackForce;
    }

    public Vector3 GetRelativePosition() => relativePosition;

    public static float DegSin(float value) => Mathf.Sin((value / 180f) * Mathf.PI);

    public static float DegCos(float value) => Mathf.Cos((value / 180f) * Mathf.PI);
}