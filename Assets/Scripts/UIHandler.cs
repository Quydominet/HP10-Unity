using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    // Init
    [SerializeField] private Canvas PlayerUI;
    [SerializeField] private Transform PlayerRoot;
    [SerializeField] private Transform PlayerCam;

    [Header("Radar Settings")]
    public float radarRange = 50f;
    public float radarSize = 156f;

    [Header("Speed Settings")]
    private readonly float MinAngle = 159.7f;
    private readonly float MaxAngle = -92.42f;

    [Header("UI Mode Settings")]
    public bool ToggleSpeedo = false;

    [Header("Weapon Settings")]
    [SerializeField] private RectTransform AmmoIconPrefab;
    public Projectile DisplayedWeapon;

    [Header("Targets")]
    public Transform[] targets;         // World objects to track
    public RectTransform blipPrefab;    // Prefab for blip icon
    private RectTransform[] blips;      // Instantiated blips

    [Header("Ammo Belt Effect")]
    [SerializeField] private float shrinkSpeed = 50f;
    [SerializeField] private float cellWidth = 8.67f; // match your icon size in the Inspector

    private List<Vector2> iconBasePositions = new List<Vector2>();
    private List<Image> ammoIcons = new List<Image>();
    private List<LayoutElement> ammoLayouts = new List<LayoutElement>(); // missing declaration
    private int lastBuiltMaxAmmo = -1;

    // Status Frame
    private Transform StatusFrame;
    private Health HealthScript;

    private Transform Health;
    private Image HealthBarImage;
    private RectTransform HealthBarRect;
    private Image HealthBgImage;

    private Transform Radar;
    private Transform BlipContainer;

    private Transform Nitro;
    private RectTransform NitroBarRect;

    private Transform Speed;
    private Transform SpeedArrow;
    private TextMeshProUGUI SpeedText;
    private TextMeshProUGUI GearText;

    private Transform Markers;
    private Transform MarkTemplate;

    // Points Frame
    private Transform Point;
    private TextMeshProUGUI PointText;
    private int CurrentPoint = 0;

    // Weapon Frame
    private Transform WeaponFrame;
    private Transform AmmoContainer;

    private Transform Crosshair;
    private RectTransform CrosshairRect;

    // Cached car ref
    private CarController car;

    Vector3 LerpPos(Vector3 start, Vector3 end, float t) => start + (end - start) * t;
    private IEnumerator FlashImage(Image img)
    {
        Color original = img.color;
        Color transparent = original;
        transparent.a = 0f;

        img.color = transparent;
        yield return new WaitForSeconds(0.1f);
        img.color = original;
    }
    public void FlashInfiniteAmmoIcon()
    {
        if (AmmoContainer.childCount <= 1) return;
        Image firstImage = AmmoContainer.GetChild(0).GetComponent<Image>();
        StartCoroutine(FlashImage(firstImage));
    }
    void UpdateHealth()
    {
        float healthPercent = HealthScript.GetHealth() / HealthScript.GetMaxHealth();

        HealthBarRect.localScale = new Vector3(healthPercent, 1f, 1f);
        HealthBarImage.color = Color.Lerp(
            new Color(128f / 255f, 38f / 255f, 38f / 255f),    // dark red
            new Color(47f / 255f, 128f / 255f, 38f / 255f),   // green
            healthPercent
        );
        HealthBgImage.color = Color.Lerp(
            new Color(63f / 255f, 19f / 255f, 19f / 255f),     // dark red
            new Color(37f / 255f, 71f / 255f, 38f / 255f),     // green
            healthPercent
        );
    }
    void UpdateBlip(Transform target, RectTransform blip)
    {
        Vector3 offset = target.position - PlayerRoot.position;

        Quaternion rotation = Quaternion.Euler(0f, -PlayerCam.eulerAngles.y, 0f);
        Vector3 rotatedOffset = rotation * offset;

        blip.anchoredPosition = new Vector2(rotatedOffset.x, rotatedOffset.z) / radarRange * (radarSize / 2f);
    }
    void UpdateRadar()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
            {
                blips[i].gameObject.SetActive(false);
                blips[i] = null;
                continue;
            }

            UpdateBlip(targets[i], blips[i]);
        }
    }
    private IEnumerator DeferredSetActive(GameObject obj, bool state)
    {
        yield return null; // wait one frame for layout to finish
        if (obj != null) obj.SetActive(state);
    }
    void RebuildAmmoIcons(int maxAmmo)
    {
        foreach (Transform child in AmmoContainer)
            Destroy(child.gameObject);

        ammoIcons.Clear();
        ammoLayouts.Clear(); // clear this too
        iconBasePositions.Clear();

        int iconsToCreate = maxAmmo == int.MaxValue ? 1 : maxAmmo;

        for (int i = 0; i < iconsToCreate; i++)
        {
            RectTransform icon = Instantiate(AmmoIconPrefab, AmmoContainer);
            icon.name = i.ToString();

            LayoutElement le = icon.gameObject.GetComponent<LayoutElement>();
            if (le == null) le = icon.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = cellWidth;
            le.minWidth = 0f;

            ammoIcons.Add(icon.GetComponent<Image>());
            ammoLayouts.Add(le); // cache it
            iconBasePositions.Add(icon.anchoredPosition);
        }
    }
    void UpdateAmmo()
    {
        if (DisplayedWeapon == null) return;
        int currentAmmo = DisplayedWeapon.GetAmmo();
        int maxAmmo = DisplayedWeapon.GetMaxAmmo();

        if (maxAmmo != lastBuiltMaxAmmo)
        {
            RebuildAmmoIcons(maxAmmo);
            lastBuiltMaxAmmo = maxAmmo;

            for (int i = 0; i < ammoIcons.Count; i++)
            {
                ammoIcons[i].rectTransform.anchoredPosition = iconBasePositions[i];
                ammoIcons[i].rectTransform.localScale = Vector3.one;
                Color c = ammoIcons[i].color;
                c.a = i < currentAmmo ? 1f : 0f;
                ammoIcons[i].color = c;
            }
        }

        if (maxAmmo == int.MaxValue) return;

        int totalConsumed = maxAmmo - currentAmmo;

        for (int i = 0; i < ammoIcons.Count; i++)
        {
            int targetSlot = i - totalConsumed;
            RectTransform rt = ammoIcons[i].rectTransform;
            LayoutElement le = ammoIcons[i].GetComponent<LayoutElement>(); // fetch per icon
            Color c = ammoIcons[i].color;

            if (targetSlot < 0)
            {
                rt.localScale = Vector3.Lerp(rt.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);
                if (le != null) le.preferredWidth = Mathf.Lerp(le.preferredWidth, 0f, shrinkSpeed * Time.deltaTime);
                c.a = Mathf.Lerp(c.a, 0f, shrinkSpeed * Time.deltaTime);

                if (rt.localScale.x < 0.3f)
                {
                    rt.localScale = Vector3.zero;
                    if (le != null) le.preferredWidth = 0f;
                    c.a = 0f;
                    if (rt.gameObject.activeSelf)
                        StartCoroutine(DeferredSetActive(rt.gameObject, false));
                }
            }
            else
            {
                if (!rt.gameObject.activeSelf)
                    StartCoroutine(DeferredSetActive(rt.gameObject, true));

                rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one, shrinkSpeed * Time.deltaTime);
                if (le != null) le.preferredWidth = Mathf.Lerp(le.preferredWidth, cellWidth, shrinkSpeed * Time.deltaTime);
                c.a = Mathf.Lerp(c.a, 1f, shrinkSpeed * Time.deltaTime);

                if (rt.localScale.x > 0.99f)
                {
                    rt.localScale = Vector3.one;
                    if (le != null) le.preferredWidth = cellWidth;
                    c.a = 1f;
                }
            }

            ammoIcons[i].color = c;
        }
    }
    void UpdateNitro()
    {
        if (car == null) return;
        float nitroPercent = car.CurrentNitrous / car.NitrousCapacity;
        NitroBarRect.localScale = new Vector3(nitroPercent, 1f, 1f);
    }
    void UpdateSpeed()
    {
        if (car == null || SpeedText == null) return;

        float speed = car.GetSpeed();
        int gear = car.CurrentGear;

        float maxSpeed = ToggleSpeedo
            ? car.GetTopSpeed()
            : (gear >= 0 ? car.gears[gear].maxSpeed : car.reverseGear.maxSpeed);

        SpeedArrow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(MinAngle, MaxAngle, speed / maxSpeed));
        SpeedText.text = $"<mspace=.5em>{Mathf.RoundToInt(speed * 3.6f):D3}";

        GearText.text = gear == -1 ? "R" : $"{gear + 1}";
    }
    void UpdateCrosshair()
    {
        if (DisplayedWeapon == null) return;

        Transform barrel = DisplayedWeapon.BarrelPos;
        Ray ray = new Ray(barrel.position, barrel.forward);

        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, 100f) ? hit.point : ray.GetPoint(100f);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPoint);

        CrosshairRect.position = LerpPos(CrosshairRect.position, screenPos, 10f * Time.deltaTime);
    }
    void CreateMarkers()
    {
        //Cleanup
        foreach (Transform child in Markers)
        {
            if (child == MarkTemplate) continue;
            Destroy(child.gameObject);
        }

        if (ToggleSpeedo)
        {
            float maxSpeed = Mathf.Ceil(car.GetTopSpeed()*3.6f/10)*10;
            for (int speed = 0; speed <= maxSpeed; speed+=2)
            {
                float speedFraction = speed / maxSpeed;
                float angle = Mathf.Lerp(MinAngle, MaxAngle, speedFraction);
                Transform marker = Instantiate(MarkTemplate, Markers);
                marker.name = "Marker_" + speed;
                marker.localRotation = Quaternion.Euler(0f, 0f, angle);

                if (speed % 20 == 0)
                {
                    Transform MarkerImg = marker.Find("Mark");
                    MarkerImg.gameObject.SetActive(true);
                }
                else
                {
                    Transform MarkerImg = marker.Find("SubMark");
                    MarkerImg.gameObject.SetActive(true);
                }

                marker.gameObject.SetActive(true);
            }
        }
        else
        {
            int maxRPM = 8000;
            int step = 250;

            for (int rpm = 0; rpm <= maxRPM; rpm += step)
            {
                float rpmFraction = (float)rpm / maxRPM;
                float angle = Mathf.Lerp(MinAngle, MaxAngle, rpmFraction);

                Transform marker = Instantiate(MarkTemplate, Markers);
                marker.name = "Marker_" + rpm;
                marker.localRotation = Quaternion.Euler(0f, 0f, angle);

                if (rpm % 1000 == 0)
                {
                    marker.Find("Mark").gameObject.SetActive(true);
                }
                else
                {
                    marker.Find("SubMark").gameObject.SetActive(true);
                }

                marker.gameObject.SetActive(true);
            }
        }
    }
    void UpdatePoints()
    {
        if (PointText == null) return;
        PointText.text = "Points: " + CurrentPoint;
    }
    void RenewBlips()
    {
        blips = new RectTransform[targets.Length];
        for (int i = 0; i < targets.Length; i++)
            blips[i] = Instantiate(blipPrefab, BlipContainer);
    }
    void Awake()
    {
        // Cache car
        car = GetComponent<CarController>();

        // Status frame
        StatusFrame = PlayerUI.transform.Find("Status");
        HealthScript = GetComponent<Health>();

        Transform healthTransform = StatusFrame.Find("Health");
        HealthBgImage = healthTransform.GetComponent<Image>();
        Transform bar = healthTransform.Find("Bar");
        HealthBarRect = bar.GetComponent<RectTransform>();
        HealthBarImage = bar.GetComponent<Image>();

        Transform nitro = StatusFrame.Find("Nitro");
        NitroBarRect = nitro.Find("Bar").GetComponent<RectTransform>();

        Radar = StatusFrame.Find("Radar");
        BlipContainer = Radar.Find("BlipContainer");

        // Speed frame
        Transform speedFrame = PlayerUI.transform.Find("Speed");
        SpeedArrow = speedFrame.Find("Arrow");
        SpeedText = speedFrame.Find("Number").GetComponent<TextMeshProUGUI>();
        GearText = speedFrame.Find("Gear").GetComponent<TextMeshProUGUI>();
        Markers = speedFrame.Find("Markers");
        MarkTemplate = Markers.Find("Template");

        // Points frame
        Point = PlayerUI.transform.Find("Points");
        PointText = Point.Find("Number").GetComponent<TextMeshProUGUI>();

        // Weapon frame
        WeaponFrame = PlayerUI.transform.Find("Weapon");
        AmmoContainer = WeaponFrame.Find("AmmoContainer");

        // Crosshair
        Crosshair = PlayerUI.transform.Find("Crosshair");
        CrosshairRect = Crosshair.GetComponent<RectTransform>();

        RenewBlips();
    }
    void Start()
    {
        // All Awakes are done — safe to read from other components
        UpdateHealth();
        UpdatePoints();
        CreateMarkers();
    }
    void Update()
    {
        UpdateNitro();
        UpdateRadar();
        UpdateAmmo();
        UpdateSpeed();
        UpdateCrosshair();
    }
    void NPCDeath()
    {
        CurrentPoint += 5;
        UpdatePoints();
    }
    void HealthChange() => UpdateHealth();
}