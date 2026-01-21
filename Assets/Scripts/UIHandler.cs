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

    [Header("Targets")]
    public Transform[] targets;             // World objects to track
    public RectTransform blipPrefab;        // Prefab for blip icon
    private RectTransform[] blips;          // Instantiated blips


    // Status Frame
    private Transform StatusFrame;
    private Health HealthScript;

    private Transform HealthBar;
    private Transform Bar;

    private Transform Radar;
    private Transform BlipContainer;
    private Transform RadarArrow;

    // Weapon Frame
    private Transform WeaponFrame;
    // Code
    void RenewBlips()
    {
        blips = new RectTransform[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            RectTransform blip = Instantiate(blipPrefab, BlipContainer);
            blips[i] = blip;
        }
    }
    void Awake()
    {
        StatusFrame = PlayerUI.transform.Find("Status");
        HealthScript = gameObject.GetComponent<Health>();

        HealthBar = StatusFrame.Find("Health");
        Bar = HealthBar.Find("Bar");

        Radar = StatusFrame.Find("Radar");
        RadarArrow = Radar.Find("PlayerArrow");
        BlipContainer = Radar.Find("BlipContainer");

        WeaponFrame = PlayerUI.transform.Find("Weapon");
        RenewBlips();
    }
    void UpdateHealth()
    {
        float healthPercent = HealthScript.GetHealth() / HealthScript.GetMaxHealth();

        Bar.GetComponent<RectTransform>().localScale = new Vector3(healthPercent, 1, 1);
        Bar.GetComponent<Image>().color = Color.Lerp(
            new Color(128f / 255f, 38f / 255f, 38f / 255f),   // dark red
            new Color(47f / 255f, 128f / 255f, 38f / 255f),   // green
            healthPercent                             // 0 → red, 1 → green
        );

        HealthBar.GetComponent<Image>().color = Color.Lerp(
            new Color(63f / 255f, 19f / 255f, 19f / 255f),   // dark red
            new Color(37f / 255f, 71f / 255f, 38f / 255f),   // green
            healthPercent                             // 0 → red, 1 → green
        );
    }
    void UpdateBlip(Transform target, RectTransform blip)
    {
        Vector3 offset = target.position - PlayerRoot.position;

        float cameraYaw = PlayerCam.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, -cameraYaw, 0);
        Vector3 rotatedOffset = rotation * offset;
        
        Vector2 radarPos = new Vector2(rotatedOffset.x, rotatedOffset.z) / radarRange * (radarSize / 2);

        blip.anchoredPosition = radarPos;
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
    void Update()
    {
        UpdateHealth();
        UpdateRadar();
    }
}
