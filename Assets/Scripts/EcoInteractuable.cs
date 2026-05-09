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
                Debug.Log("Está muy oscuro... no alcanzo a distinguir qué es esto.");
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
        
        Debug.Log("Acertijo abierto. El tiempo está congelado.");
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
        
        Debug.Log("Rompecabezas cerrado. El reloj vuelve a correr.");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            enRango = true;
            milo = collision.GetComponent<MiloController>();
            Debug.Log("Presiona 'E' y usa tu linterna.");
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
}