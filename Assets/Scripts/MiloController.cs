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
    
    // NUEVO: Le restamos 90 grados porque la luz de Unity nace apuntando arriba
    public float compensacionAngulo = -90f; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // NUEVO: Forzamos al objeto a apagarse (o prenderse) desde el milisegundo 1
        if (luzLinterna != null)
        {
            luzLinterna.SetActive(linternaEncendida);
        }
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

        if (movimiento != Vector2.zero && luzLinterna != null)
        {
            // Calculamos el ángulo normal y le sumamos la compensación
            float angulo = Mathf.Atan2(movimiento.y, movimiento.x) * Mathf.Rad2Deg;
            luzLinterna.transform.rotation = Quaternion.Euler(0, 0, angulo + compensacionAngulo);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            linternaEncendida = !linternaEncendida;
            if (luzLinterna != null)
            {
                luzLinterna.SetActive(linternaEncendida);
            }

            if (linternaEncendida)
            {
                MensajeriaJugador.Mostrar("Linterna encendida.");
            }
            else
            {
                MensajeriaJugador.Mostrar("Linterna apagada.");
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);
    }
}