using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public static Gun Instance;
    
    [Header("Textures")]
    public Texture[] ADSTextures;
    public Texture[] BlindFireTextures;
    public Texture[] ADSFireTextures;
    public Texture[] ReloadTextures;

    [Header("Material")]
    public Material GunMaterial;

    [Header("Prefabs")]
    public GameObject GunFlashPrefab;
    public GameObject WallHitPrefab;

    [Header("Intervals")]
    public float ADSInterval;
    public float ShootInterval;
    public float ReloadInterval;

    [Header("Accuracy")]
    public float BlindInaccuracy;
    public float ADSInaccuracy;
    public float FOVDifference;

    [Header("Ammo Values")]
    public int ClipSize;
    public int LoadedAmmo;
    public int UnloadedAmmo;

    [Header("Audio")]
    public AudioClip ShotClip;
    public AudioClip ReloadClip;
    public AudioClip EmptyClip;

    [HideInInspector] public bool Shooting;

    private Vector3 startPosition;
    private Camera cam;
    private GameObject flashlight;
    private AudioSource audioSource;
    private AudioSource audioSourceOverdrive;

    private bool blockADS;
    private bool lockADS;
    private int adsIndex;
    private float adsTimer;

    private float oFOV;
    private float oSpeed;
    private float oCamSpeed;

    private int shootIndex;
    private float shootTimer;

    private bool reloading;
    private int reloadIndex;
    private float reloadTimer;

    void Start()
    {
        if (!Instance) Instance = this;
        else Destroy(this);

        startPosition = transform.localPosition;
        cam = Camera.main;
        flashlight = transform.GetChild(0).gameObject;

        oFOV = cam.fieldOfView;
        oSpeed = Player.Instance.MovementSpeed;
        oCamSpeed = CameraController.Instance.RotationSpeed;

        audioSource = GetComponent<AudioSource>();
        audioSourceOverdrive = GetComponents<AudioSource>()[1];

        UI.Instance.UpdateAmmoBox();

        
    }

    void Update()
    {
        if (UI.Instance.Paused) return;

        if (Player.Instance.Dead)
        {
            flashlight.SetActive(false);
            transform.position += transform.up * Time.deltaTime * Player.Instance.Rigbod.velocity.y;
            return;
        }

        if (Player.Instance.GodMode) UnloadedAmmo = 120;

        transform.localPosition = startPosition;
        transform.position -= CameraController.Instance.GetRelativePosition();

        HandleADS();
        HandleShoot();
        HandleReload();
    }
    
    private void HandleADS()
    {
        if (Time.time > adsTimer)
        {
            adsTimer = Time.time + ADSInterval;

            if (Input.GetMouseButton(1) && !blockADS)
            {
                if (adsIndex < 3) adsIndex++;
            }
            else if (!lockADS)
            {
                if (adsIndex > 0) adsIndex--;
            }
        }

        if (Input.GetMouseButton(1) && !blockADS)
        {
            if (cam.fieldOfView > oFOV - FOVDifference + Time.deltaTime * 100) cam.fieldOfView -= Time.deltaTime * FOVDifference / (ADSInterval * 8);
            else cam.fieldOfView = oFOV - FOVDifference;

            if (Player.Instance.MovementSpeed > oSpeed * 0.5f) Player.Instance.MovementSpeed -= Time.deltaTime * (oSpeed * 0.5f) / (ADSInterval * 8);
            else Player.Instance.MovementSpeed = oSpeed * 0.5f;

            if (CameraController.Instance.RotationSpeed > oCamSpeed * 0.5f) CameraController.Instance.RotationSpeed -= Time.deltaTime * (oCamSpeed * 0.5f) / (ADSInterval * 8);
            else CameraController.Instance.RotationSpeed = oCamSpeed * 0.5f;
        }
        else if (!lockADS)
        {
            if (cam.fieldOfView < oFOV - Time.deltaTime * 100) cam.fieldOfView += Time.deltaTime * FOVDifference / (ADSInterval * 8);
            else cam.fieldOfView = oFOV;

            if (Player.Instance.MovementSpeed < oSpeed) Player.Instance.MovementSpeed += Time.deltaTime * (oSpeed * 0.5f) / (ADSInterval * 8);
            else Player.Instance.MovementSpeed = oSpeed;

            if (CameraController.Instance.RotationSpeed < oCamSpeed) CameraController.Instance.RotationSpeed += Time.deltaTime * (oCamSpeed * 0.5f) / (ADSInterval * 8);
            else CameraController.Instance.RotationSpeed = oCamSpeed;
        }

        if (!Shooting && (!reloading || (reloading && adsIndex > 0))) GunMaterial.mainTexture = ADSTextures[adsIndex];
    }

    private void HandleShoot()
    {
        if (Input.GetMouseButtonDown(0) && LoadedAmmo == 0 && UnloadedAmmo == 0)
        {
            audioSource.volume = 0.6f;
            audioSource.PlayOneShot(EmptyClip);
        }
        if (Input.GetMouseButtonDown(0) && (adsIndex == 0 || adsIndex == 3) && !Shooting && !reloading && LoadedAmmo > 0)
        {
            Shooting = true;
            if (adsIndex == 0) blockADS = true;
            else lockADS = true;
        }
        else if (LoadedAmmo == 0 && UnloadedAmmo > 0)
        {
            reloading = true;
            blockADS = true;
        }

        if (Shooting)
        {
            if (Time.time > shootTimer)
            {
                shootTimer = Time.time + ShootInterval;

                if (shootIndex == 0) Shoot(); 

                if (adsIndex == 0) GunMaterial.mainTexture = BlindFireTextures[shootIndex];
                else  GunMaterial.mainTexture = ADSFireTextures[shootIndex];
                shootIndex++;

                if (shootIndex == ADSFireTextures.Length)
                {
                    Shooting = false;
                    blockADS = false;
                    lockADS = false;
                    adsTimer = 0;
                    shootTimer = 0;
                    shootIndex = 0;
                }
            }
        }
    }

    private void HandleReload()
    {
        if (Input.GetButtonDown("Reload") && !Shooting && !reloading && LoadedAmmo < ClipSize && UnloadedAmmo > 0)
        {
            blockADS = true;
            reloading = true;
        }

        if (reloading && adsIndex == 0)
        {
            if (Time.time > reloadTimer)
            {
                reloadTimer = Time.time + ReloadInterval;

                if (reloadIndex == 0)
                {
                    audioSource.volume = 1;
                    audioSource.PlayOneShot(ReloadClip);
                }

                GunMaterial.mainTexture = ReloadTextures[reloadIndex];
                reloadIndex++;

                if (reloadIndex == ReloadTextures.Length)
                {
                    int newBullets = ClipSize - LoadedAmmo;
                    newBullets = Mathf.Clamp(newBullets, 0, UnloadedAmmo);
                    LoadedAmmo += newBullets;
                    UnloadedAmmo -= newBullets;
                    UI.Instance.UpdateAmmoBox();

                    reloading = false;
                    blockADS = false;
                    adsTimer = 0;
                    reloadTimer = 0;
                    reloadIndex = 0;
                }
            }
        }
    }

    private void Shoot()
    {
        LoadedAmmo--;
        UI.Instance.UpdateAmmoBox();

        audioSourceOverdrive.PlayOneShot(ShotClip);

        CameraController.Instance.AddKnockback();

        Instantiate(GunFlashPrefab, transform.position, Quaternion.identity);

        Ray bulletPath = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

        Vector3 drift = UnityEngine.Random.insideUnitSphere;
        if (adsIndex == 0) bulletPath.direction += drift * BlindInaccuracy;
        else  bulletPath.direction += drift * ADSInaccuracy;

        Physics.Raycast(bulletPath, out RaycastHit hit);

        if (!hit.collider) return;

        int hitLayer = hit.collider.gameObject.layer;

        if (hitLayer == 8 || hitLayer == 9)
        {
            Zombie zombie;

            if (hitLayer == 8)
            {
                zombie = hit.collider.transform.parent.GetComponent<Zombie>();
                zombie.TakeDamage(false, hit.point);
            }
            else
            {
                zombie = hit.collider.transform.parent.parent.GetComponent<Zombie>();
                zombie.TakeDamage(true, hit.point);
            }

            return;
        }

        if (hit.rigidbody) hit.rigidbody.AddForceAtPosition(bulletPath.direction.normalized * 2, hit.point, ForceMode.Impulse);

        Instantiate(WallHitPrefab, hit.point, Quaternion.identity);
        Instantiate(GunFlashPrefab, hit.point - bulletPath.direction.normalized * 0.1f, Quaternion.identity);
    }

    public void ChangeCrosshair()
    {
        UI.Instance.Crosshair.color = new Color(1, 0, 0, 0.25f);
        UI.Instance.Crosshair.transform.eulerAngles = new Vector3(0, 0, 45);
        CancelInvoke();
        Invoke("ResetCrosshair", 0.35f);
    }

    private void ResetCrosshair()
    {
        UI.Instance.Crosshair.color = new Color(1, 1, 1, 0.25f);
        UI.Instance.Crosshair.transform.eulerAngles = Vector3.zero;
    }
}