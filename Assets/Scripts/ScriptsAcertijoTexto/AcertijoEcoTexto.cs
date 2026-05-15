using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class AcertijoEcoTexto : MonoBehaviour
{
    [Header("El Secreto")]
    [Tooltip("Escribe aquí la palabra o frase correcta")]
    public string respuestaCorrecta = "receta"; 

    [Header("Referencias")]
    public GameObject panelAcertijo;
    public TMP_InputField cuadroDeTexto;
    public GameObject ecoEnEscena;
    public MiloController milo;

    [Header("Interacción")]
    public KeyCode teclaInteraccion = KeyCode.E;
    private bool jugadorEnRango = false;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (panelAcertijo != null) panelAcertijo.SetActive(false);
    }

    void Update()
    {
        // Solo escuchamos la tecla E si el jugador está en rango
        if (jugadorEnRango && Input.GetKeyDown(teclaInteraccion))
        {
            // Si el panel está apagado, lo abrimos
            if (!panelAcertijo.activeSelf)
            {
                AbrirPanel();
            }
            // Si el panel ESTÁ abierto, pero NO estamos escribiendo adentro del cuadro, lo cerramos
            else if (panelAcertijo.activeSelf && !cuadroDeTexto.isFocused)
            {
                CerrarPanelSinResolver();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) jugadorEnRango = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) jugadorEnRango = false;
    }

    void AbrirPanel()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.OcultarHudProgresoEnMinijuego();
        }

        panelAcertijo.SetActive(true);
        cuadroDeTexto.text = ""; 
        
        // CONGELAMIENTO TOTAL: Detiene el tiempo, las físicas, a Milo y a los enemigos
        Time.timeScale = 0f;
        if (milo != null) milo.puedeMoverse = false;
    }

    public void VerificarRespuesta()
    {
        string respuestaJugador = cuadroDeTexto.text.Trim().ToLower();
        string respuestaSecreta = respuestaCorrecta.Trim().ToLower();

        if (respuestaJugador == respuestaSecreta)
        {
            Debug.Log("¡Correcto! Milo recogió el Eco.");

            if (gameManager != null)
            {
                gameManager.RegistrarEcoRecolectado();
            }
            
            // 1. Apagamos la UI
            panelAcertijo.SetActive(false);

            // 2. Quitamos el foco de la UI
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            // 3. DESCONGELAMIENTO: Restauramos el tiempo y a Milo
            Time.timeScale = 1f;
            if (milo != null) milo.puedeMoverse = true;

            if (gameManager != null)
            {
                gameManager.RestaurarHudProgresoTrasMinijuego();
            }

            // 4. Destruimos el objeto de la escena
            if (ecoEnEscena != null) Destroy(ecoEnEscena);
        }
        else
        {
            Debug.Log("Incorrecto. Intenta de nuevo.");
            cuadroDeTexto.text = ""; 
            
            // Mantenemos el recuadro activo para que el jugador pueda seguir escribiendo de inmediato
            cuadroDeTexto.ActivateInputField();
        }
    }

    // Esta función se activa si presionan 'E' para salir, o si la conectas al botón de una "X"
    public void CerrarPanelSinResolver()
    {
        panelAcertijo.SetActive(false);
        
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // DESCONGELAMIENTO: Restauramos el tiempo y a Milo
        Time.timeScale = 1f;
        if (milo != null) milo.puedeMoverse = true;

        if (gameManager != null)
        {
            gameManager.RestaurarHudProgresoTrasMinijuego();
        }
    }
}