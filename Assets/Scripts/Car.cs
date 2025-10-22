using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField]
    private float tocdoxe = 100f;
    [SerializeField]
    private float lucReXe = 100f;
    [SerializeField]
    private float lucPhanh = 50f;
    [SerializeField]
    private GameObject hieuUngPhanh;
    private float dauvaodichuyen;
    private float dauVaoRe;
    private Rigidbody rb;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        dauvaodichuyen = Input.GetAxis("Vertical");
        dauVaoRe = Input.GetAxis("Horizontal");
        Dichuyenxe();
        ReXe();
        if (dauvaodichuyen > 0&& Input.GetKey(KeyCode.LeftShift))
        {
            PhanhXe();
        }
    }

    public void Dichuyenxe()
    {
        rb.AddRelativeForce(Vector3.forward * dauvaodichuyen * tocdoxe);
        hieuUngPhanh.SetActive(false);
    }

    public void ReXe()
    {
        Quaternion re = Quaternion.Euler(Vector3.up * dauVaoRe * lucReXe * Time.deltaTime);
        rb.MoveRotation(rb.rotation * re);
    }
    public void PhanhXe()
    {
        if (rb.linearVelocity.z!= 0)
        {
            rb.AddRelativeForce(-Vector3.forward * lucPhanh);
            hieuUngPhanh.SetActive(true);
        }
    }
}
