using UnityEngine;

public class EcoInteractuable : MonoBehaviour
{
    public string nombreEco = "Oso de Peluche";
    
    [Header("Interfaz del Rompecabezas")]
    // Aquí conectaremos el panel visual que vamos a crear en Unity
    public GameObject panelRompecabezas; 
    
    private bool enRango = false;
    private MiloController milo;

    void Update()
    {
        if (enRango && Input.GetKeyDown(KeyCode.E))
        {
            if (milo != null && milo.linternaEncendida)
            {
                AbrirRompecabezas();
            }
            else
            {
                MensajeriaJugador.Mostrar("Está muy oscuro. Enciende la linterna para inspeccionar.");
            }
        }
    }

    void AbrirRompecabezas()
    {
        milo.puedeMoverse = false;
        
        // Magia negra de Unity: Congela el tiempo y las físicas de todo el juego
        Time.timeScale = 0f; 
        
        // Prendemos el panel visual
        if (panelRompecabezas != null)
        {
            panelRompecabezas.SetActive(true); 
        }
        
        MensajeriaJugador.Mostrar("Acertijo abierto.");
    }

    // Esta función la llamaremos desde un botón en la pantalla para salir
    public void CerrarRompecabezas()
    {
        // Descongelamos el universo para que todo vuelva a la normalidad
        Time.timeScale = 1f; 
        milo.puedeMoverse = true;
        
        // Apagamos el panel visual
        if (panelRompecabezas != null)
        {
            panelRompecabezas.SetActive(false); 
        }
        
        MensajeriaJugador.Mostrar("Volviste al juego principal.");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            enRango = true;
            milo = collision.GetComponent<MiloController>();
            MostrarMensajeInteraccion();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            enRango = false;
            milo = null;
        }
    }

    void MostrarMensajeInteraccion()
    {
        if (milo != null && milo.linternaEncendida)
        {
            MensajeriaJugador.Mostrar("Presiona E para interactuar con este eco.");
        }
        else
        {
            MensajeriaJugador.Mostrar("Enciende la linterna y presiona E para interactuar con este eco.");
        }
    }
}