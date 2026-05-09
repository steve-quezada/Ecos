using UnityEngine;

public class MiloController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 5f;
    public bool puedeMoverse = true; 

    private Rigidbody2D rb;
    private Vector2 movimiento;

    [Header("Linterna")]
    public bool linternaEncendida = false;
    public GameObject luzLinterna;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!puedeMoverse)
        {
            movimiento = Vector2.zero; 
            return; 
        }

        movimiento.x = Input.GetAxisRaw("Horizontal");
        movimiento.y = Input.GetAxisRaw("Vertical");
        movimiento = movimiento.normalized;

        // NUEVO: Hacemos que la linterna gire hacia donde camina Milo
        if (movimiento != Vector2.zero && luzLinterna != null)
        {
            // Calculamos el ángulo en base a las teclas que está presionando
            float angulo = Mathf.Atan2(movimiento.y, movimiento.x) * Mathf.Rad2Deg;
            // Aplicamos la rotación al objeto de la luz
            luzLinterna.transform.rotation = Quaternion.Euler(0, 0, angulo);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            linternaEncendida = !linternaEncendida;
            if (luzLinterna != null)
            {
                luzLinterna.SetActive(linternaEncendida);
            }
            Debug.Log("Linterna: " + (linternaEncendida ? "ON" : "OFF"));
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);
    }
}