using UnityEngine;

public class EstaticaController : MonoBehaviour
{
    [Header("Límites de la Habitación (NUEVO)")]
    public float limiteMinX = -7f;
    public float limiteMaxX = 7f;
    public float limiteMinY = -4f;
    public float limiteMaxY = 4f;

    [Header("Configuración de Caza (Ruido)")]
    public float radioDeteccion = 3f;
    public int ruidoParaCazar = 5; // Asegúrate de que esto esté en 5 en el Inspector
    public float velocidadBase = 1.5f; 
    public float incrementoVelocidadPorRuido = 0.4f; 
    
    [Header("Aparición Aleatoria (15s)")]
    public float tiempoAparicion = 15f; 
    public float duracionPatrullaje = 5f; 
    private float temporizadorAparicion = 0f;
    private float temporizadorPatrullaje = 0f;

    [Header("Patrullaje")]
    public float rangoMovimientoAleatorio = 5f;
    private Vector2 puntoDestinoAleatorio;

    private Transform jugador;
    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;
    private Collider2D colisionador;
    
    private bool estaActiva = false;
    private bool estaCazandoPorRuido = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();

        DesactivarEstatica();
    }

    void Update()
    {
        // Revisamos que el ManagerRuido exista para evitar errores
        if (ManagerRuido.instancia == null) return;

        float nivelDeRuido = ManagerRuido.instancia.ruidoActual;
        float ruidoMaximo = ManagerRuido.instancia.ruidoMaximo;

        // 1. REVISAR CASTIGO MÁXIMO (Ruido en 15)
        if (nivelDeRuido >= ruidoMaximo)
        {
            transform.position = jugador.position;
            ActivarEstatica();
            Debug.Log("¡RUIDO AL MÁXIMO! La Estática cayó directamente sobre Milo.");
            this.enabled = false; 
            return; 
        }

        // 2. LÓGICA DE CAZA POR RUIDO ALTO (Furia nivel 5+)
        if (nivelDeRuido >= ruidoParaCazar)
        {
            estaCazandoPorRuido = true;
            if (!estaActiva) ActivarEstatica(); 
            PerseguirAMilo();
            return; 
        }
        else 
        {
            if (estaCazandoPorRuido)
            {
                estaCazandoPorRuido = false;
                DesactivarEstatica();
            }
        }

        // 3. LÓGICA DEL RELOJ DE 15 SEGUNDOS Y DETECCIÓN MENOR
        if (!estaActiva)
        {
            temporizadorAparicion += Time.deltaTime;
            if (temporizadorAparicion >= tiempoAparicion)
            {
                IniciarRecorridoAleatorio();
                temporizadorAparicion = 0f; 
            }
        }
        else
        {
            float distanciaAMilo = Vector2.Distance(transform.position, jugador.position);

            // ACTUALIZADO: Compara con el ruido del ManagerRuido
            if (!gameManager.miloOculto && (nivelDeRuido > 0 || distanciaAMilo <= radioDeteccion))
            {
                PerseguirAMilo();
                temporizadorPatrullaje = 0f; 
            }
            else
            {
                PatrullarHabitacion();
                temporizadorPatrullaje += Time.deltaTime;
                if (temporizadorPatrullaje >= duracionPatrullaje)
                {
                    DesactivarEstatica(); 
                }
            }
        }
    }

    void IniciarRecorridoAleatorio()
    {
        ActivarEstatica();
        temporizadorPatrullaje = 0f; 
        
        float xA = Random.Range(limiteMinX, limiteMaxX);
        float yA = Random.Range(limiteMinY, limiteMaxY);
        transform.position = new Vector2(xA, yA);
        
        AsignarNuevoDestinoAleatorio();
        Debug.Log("¡ALARMA! Recorrido aleatorio de 15 segundos.");
    }

    void ActivarEstatica()
    {
        estaActiva = true;
        spriteRenderer.enabled = true;
        colisionador.enabled = true;
    }

    void DesactivarEstatica()
    {
        estaActiva = false;
        spriteRenderer.enabled = false;
        colisionador.enabled = false;
    }

    void PerseguirAMilo()
    {
        float nivelDeRuido = ManagerRuido.instancia.ruidoActual;
        float velocidadExtra = (nivelDeRuido - ruidoParaCazar) * incrementoVelocidadPorRuido;
        // Evitamos que la velocidad sea negativa si el ruido baja
        if (velocidadExtra < 0) velocidadExtra = 0; 
        
        float velocidadActual = velocidadBase + velocidadExtra;
        transform.position = Vector2.MoveTowards(transform.position, jugador.position, velocidadActual * Time.deltaTime);
    }

    void PatrullarHabitacion()
    {
        transform.position = Vector2.MoveTowards(transform.position, puntoDestinoAleatorio, velocidadBase * Time.deltaTime);
        
        if (Vector2.Distance(transform.position, puntoDestinoAleatorio) < 0.1f)
        {
            AsignarNuevoDestinoAleatorio();
        }
    }

    void AsignarNuevoDestinoAleatorio()
    {
        float xAleatorio = Random.Range(limiteMinX, limiteMaxX);
        float yAleatorio = Random.Range(limiteMinY, limiteMaxY);
        puntoDestinoAleatorio = new Vector2(xAleatorio, yAleatorio);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !gameManager.miloOculto)
        {
            Debug.Log("¡GAME OVER! La Estática te alcanzó.");
        }
    }
}