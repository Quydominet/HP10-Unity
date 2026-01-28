using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIHandler : MonoBehaviour
{
    // Init
    [SerializeField] private Canvas PlayerUI;
    [SerializeField] private Transform PlayerRoot;
    [SerializeField] private Transform PlayerCam;

    [Header("Radar Settings")]
    public float radarRange = 50f;
    public float radarSize = 156f;

    [Header("Weapon Settings")]
    [SerializeField] private RectTransform AmmoIconPrefab;
    public Projectile DisplayedWeapon;

    [Header("Targets")]
    public Transform[] targets;             // World objects to track
    public RectTransform blipPrefab;        // Prefab for blip icon
    private RectTransform[] blips;          // Instantiated blips


    // Status Frame
    private Transform StatusFrame;
    private Health HealthScript;

    private Transform Health;
    private Transform HealthBar;

    private Transform Radar;
    private Transform BlipContainer;

    private Transform Nitro;
    private Transform NitroBar;

    private Transform Speed;
    private Transform SpeedNum;

    // Weapon Frame
    private Transform WeaponFrame;
    private Transform AmmoContainer;

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

        Health = StatusFrame.Find("Health");
        HealthBar = Health.Find("Bar");

        Nitro = StatusFrame.Find("Nitro");
        NitroBar = Nitro.Find("Bar");

        Radar = StatusFrame.Find("Radar");
        BlipContainer = Radar.Find("BlipContainer");

        Speed = StatusFrame.Find("Speed");
        SpeedNum = Speed.Find("Number");

        WeaponFrame = PlayerUI.transform.Find("Weapon");
        AmmoContainer = WeaponFrame.Find("AmmoContainer");
        RenewBlips();
    }
    void UpdateHealth()
    {
        float healthPercent = HealthScript.GetHealth() / HealthScript.GetMaxHealth();

        HealthBar.GetComponent<RectTransform>().localScale = new Vector3(healthPercent, 1, 1);
        HealthBar.GetComponent<Image>().color = Color.Lerp(
            new Color(128f / 255f, 38f / 255f, 38f / 255f),   // dark red
            new Color(47f / 255f, 128f / 255f, 38f / 255f),   // green
            healthPercent                             // 0 → red, 1 → green
        );

        Health.GetComponent<Image>().color = Color.Lerp(
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
    private IEnumerator FlashImage(Image img)
    {
        Color oldC = img.color;
        Color newC = oldC;
        newC.a = 0f;

        img.color = newC;                // transparent
        yield return new WaitForSeconds(0.1f); // wait 100 ms
        img.color = oldC;                // restore
    }
    public void FlashInfiniteAmmoIcon()
    {
        if (AmmoContainer.childCount <= 1) return;

        Image firstImage = AmmoContainer.GetChild(0).GetComponent<Image>();
        StartCoroutine(FlashImage(firstImage));
    }
    void UpdateAmmo()
    {
        if (DisplayedWeapon == null) return;
        int currentAmmo = DisplayedWeapon.GetAmmo();
        int maxAmmo = DisplayedWeapon.GetMaxAmmo();

        if (maxAmmo != AmmoContainer.childCount)
        {
            // Update count
            foreach (Transform child in AmmoContainer)
            {
                Destroy(child.gameObject);
            }

            if (maxAmmo == int.MaxValue)
            {
                RectTransform ammoIcon = Instantiate(AmmoIconPrefab, AmmoContainer);
                ammoIcon.name = "0";
            }
            else
            {
                for (int i = 0; i < maxAmmo; i++)
                {
                    RectTransform ammoIcon = Instantiate(AmmoIconPrefab, AmmoContainer);
                    ammoIcon.name = i.ToString();
                }
            }
        }
        else
        {
            if (maxAmmo != int.MaxValue)
            {
                for (int i = 0; i < maxAmmo; i++)
                {
                    Transform AmmoObject = AmmoContainer.Find(i.ToString());
                    if (AmmoObject == null) continue;
                    Image ammoImage = AmmoObject.GetComponent<Image>();
                    Color newC = ammoImage.color;

                    if (i < currentAmmo)
                        newC.a = 1f;
                    else
                        newC.a = 0f;

                    ammoImage.color = newC;
                }
            }
        }
    }
    /*
    void UpdateNitro()
    {
        Car car = gameObject.GetComponent<Car>();
        if (car == null) return;
        float nitroPercent = car.GetNitro() / car.GetMaxNitro();
        NitroBar.GetComponent<RectTransform>().localScale = new Vector3(nitroPercent, 1, 1);
    }
    */
    void UpdateSpeed()
    {
        float speed = gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude;
        TextMeshProUGUI text = SpeedNum.GetComponent<TextMeshProUGUI>();
        if (text == null) return;

        text.text = $"{Mathf.RoundToInt(speed)}";
    }
    void Update()
    {
        UpdateHealth();
        //UpdateNitro();
        UpdateRadar();
        UpdateAmmo();
        UpdateSpeed();
    }
}
