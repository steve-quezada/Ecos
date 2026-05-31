using UnityEngine;

public class InicioJuego : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Si dejas esto vacío, Milo aparecerá normalmente sin tirar nada.")]
    public GameObject guitarraPrefab;
    
    [Tooltip("El lugar donde Milo va a aparecer (la cama o la puerta)")]
    public Transform puntoDeAparicion; 
    
    public MiloController milo;

    private GameObject guitarraInstanciada;
    private bool miloEnCama = true;

    void Start()
    {
        if (milo == null)
        {
            Debug.LogError("InicioJuego: falta referencia de Milo en " + gameObject.name);
            enabled = false;
            return;
        }

        if (puntoDeAparicion == null)
        {
            Debug.LogError("InicioJuego: falta PuntoDeAparicion en " + gameObject.name);
            enabled = false;
            return;
        }

        // 1. Posicionamos a Milo en el lugar inicial (cama o puerta)
        milo.transform.position = puntoDeAparicion.position;

        // 2. Revisamos si HAY guitarra (es decir, si estamos en la recámara original)
        if (guitarraPrefab != null)
        {
            milo.puedeMoverse = false; // Lo bloqueamos para que no camine

            // Instanciamos la guitarra sobre él
            Vector3 posicionGuitarra = puntoDeAparicion.position + new Vector3(0, 0.5f, 0);
            guitarraInstanciada = Instantiate(guitarraPrefab, posicionGuitarra, Quaternion.identity);
            
            // Desactivamos la física de la guitarra al inicio para que no se caiga
            Rigidbody2D rbGuitarra = guitarraInstanciada.GetComponent<Rigidbody2D>();
            if (rbGuitarra != null) rbGuitarra.simulated = false;

            Debug.Log("Milo está durmiendo con su guitarra...");
        }
        else 
        {
            // 3. Si NO hay guitarra (es decir, entramos a una habitación nueva)
            milo.puedeMoverse = true; // Se puede mover de inmediato
            miloEnCama = false; // Ya no está dormido
            
            Debug.Log("Milo entró a una nueva habitación de pie.");
            
            // Apagamos este script de inmediato porque no hay guitarra que tirar
            this.enabled = false; 
        }
    }

    void Update()
    {
        // Esto solo se ejecuta si Milo empezó dormido con la guitarra
        if (miloEnCama && (Input.GetKeyDown(KeyCode.E) || Input.GetAxis("Horizontal") != 0))
        {
            LevantarseDeLaCama();
        }
    }

    void LevantarseDeLaCama()
    {
        miloEnCama = false;
        milo.puedeMoverse = true;

        // 4. La guitarra se cae
        if (guitarraInstanciada != null)
        {
            Rigidbody2D rbGuitarra = guitarraInstanciada.GetComponent<Rigidbody2D>();
            if (rbGuitarra != null)
            {
                rbGuitarra.simulated = true;
                rbGuitarra.AddForce(new Vector2(0.1f, 0f), ForceMode2D.Impulse); 
            }
        }

        // 5. ¡EL ESTRUENDO! Subimos el ruido en 5 de jalón
        if (ManagerRuido.instancia != null)
        {
            ManagerRuido.instancia.AgregarRuido(5f);
        }

        Debug.Log("¡ESTRUENDO! La guitarra cayó. El ruido subió a 5.");
        
        // Desactivamos este script para que no se repita
        this.enabled = false;
    }
}