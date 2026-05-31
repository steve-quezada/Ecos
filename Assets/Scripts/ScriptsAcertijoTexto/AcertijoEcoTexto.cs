using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [Header("UI General a ocultar")]
    [SerializeField] private GameObject uiObjetosObligatorios;
    [SerializeField] private GameObject uiEco;

    [Header("Interacción")]
    public KeyCode teclaInteraccion = KeyCode.E;

    [Header("Ajuste UI 1080p")]
    [SerializeField] private bool forzarLayout1080 = true;
    [SerializeField] private Vector2 tamanoInput1080 = new Vector2(920f, 120f);
    [SerializeField] private Vector2 posicionInput1080 = new Vector2(0f, -371.78f);
    [SerializeField] private Vector2 tamanoBotonVerificar1080 = new Vector2(320f, 80f);
    [SerializeField] private Vector2 posicionBotonVerificar1080 = new Vector2(0f, -430f);
    [SerializeField] private Vector2 tamanoBotonCerrar1080 = new Vector2(220f, 60f);
    [SerializeField] private Vector2 posicionBotonCerrar1080 = new Vector2(0f, -515f);
    [SerializeField] private int tamanoFuenteInput1080 = 40;
    [SerializeField] private int tamanoFuenteBoton1080 = 34;
    [SerializeField] private bool usarPosicionVerificarComoBano = true;
    [SerializeField] private float margenLateralVerificarComoBano = 190f;
    [SerializeField] private float margenInferiorVerificarComoBano = 52f;
    [SerializeField] private bool ocultarBotonCerrar = true;

    private bool jugadorEnRango = false;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        ResolverHudMinijuego();
        if (panelAcertijo != null) panelAcertijo.SetActive(false);
    }

    void Update()
    {
        if (panelAcertijo == null || cuadroDeTexto == null)
        {
            return;
        }

        // Solo escuchamos la tecla E si el jugador está en rango
        if (jugadorEnRango && Input.GetKeyDown(teclaInteraccion))
        {
            // Si el panel está apagado, lo abrimos
            if (!panelAcertijo.activeSelf)
            {
                if (!PuedeIniciarConLinterna())
                {
                    return;
                }

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
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        jugadorEnRango = true;

        if (milo == null)
        {
            milo = collision.GetComponent<MiloController>();
        }

        MostrarMensajeInteraccion();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) jugadorEnRango = false;
    }

    private bool PuedeIniciarConLinterna()
    {
        if (milo == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                milo = jugador.GetComponent<MiloController>();
            }
        }

        if (milo == null)
        {
            MensajeriaJugador.Mostrar("No puedes interactuar en este momento.");
            return false;
        }

        if (!milo.linternaEncendida)
        {
            MensajeriaJugador.Mostrar("Está muy oscuro. Enciende la linterna para inspeccionar.");
            return false;
        }

        return true;
    }

    private void MostrarMensajeInteraccion()
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

    void AbrirPanel()
    {
        if (panelAcertijo == null || cuadroDeTexto == null)
        {
            Debug.LogError("AcertijoEcoTexto: faltan referencias de panel o input en " + gameObject.name);
            return;
        }

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.OcultarHudProgresoEnMinijuego();
        }

        ResolverHudMinijuego();
        AplicarVisibilidadHudMinijuego(false);

        panelAcertijo.SetActive(true);
        AsegurarInteraccionYLayoutPanel();
        cuadroDeTexto.text = ""; 
        cuadroDeTexto.ActivateInputField();
        
        // CONGELAMIENTO TOTAL: Detiene el tiempo, las físicas, a Milo y a los enemigos
        Time.timeScale = 0f;
        if (milo != null) milo.puedeMoverse = false;
    }

    public void VerificarRespuesta()
    {
        if (cuadroDeTexto == null)
        {
            return;
        }

        string respuestaJugador = cuadroDeTexto.text.Trim().ToLower();
        string respuestaSecreta = respuestaCorrecta.Trim().ToLower();

        if (respuestaJugador == respuestaSecreta)
        {
            MensajeriaJugador.Mostrar("¡Correcto! Respuesta validada.");

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

            AplicarVisibilidadHudMinijuego(true);

            // 4. Destruimos el objeto de la escena
            if (ecoEnEscena != null) Destroy(ecoEnEscena);
        }
        else
        {
            MensajeriaJugador.Mostrar("Respuesta incorrecta. Intenta otra vez.");
            cuadroDeTexto.text = ""; 
            
            // Mantenemos el recuadro activo para que el jugador pueda seguir escribiendo de inmediato
            cuadroDeTexto.ActivateInputField();
        }
    }

    // Esta función se activa si presionan 'E' para salir, o si la conectas al botón de una "X"
    public void CerrarPanelSinResolver()
    {
        if (panelAcertijo == null)
        {
            return;
        }

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

        AplicarVisibilidadHudMinijuego(true);
    }

    private void AsegurarInteraccionYLayoutPanel()
    {
        AsegurarEventSystem();

        RectTransform panelRect = panelAcertijo.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelRect.localScale = Vector3.one;
        }

        CanvasGroup canvasGroup = panelAcertijo.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panelAcertijo.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        panelAcertijo.transform.SetAsLastSibling();

        if (forzarLayout1080)
        {
            AjustarLayout1080();
        }

        if (panelRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        }
    }

    private void AjustarLayout1080()
    {
        RectTransform inputRect = cuadroDeTexto.GetComponent<RectTransform>();
        if (inputRect != null)
        {
            inputRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputRect.pivot = new Vector2(0.5f, 0.5f);
            inputRect.anchoredPosition = posicionInput1080;
            inputRect.sizeDelta = tamanoInput1080;
            inputRect.localScale = Vector3.one;
        }

        if (cuadroDeTexto.textComponent != null)
        {
            cuadroDeTexto.textComponent.fontSize = tamanoFuenteInput1080;
            cuadroDeTexto.textComponent.raycastTarget = false;
        }

        if (cuadroDeTexto.placeholder is TMP_Text placeholderTMP)
        {
            placeholderTMP.fontSize = Mathf.Max(24, tamanoFuenteInput1080 - 6);
            placeholderTMP.raycastTarget = false;
        }

        AlinearContenidoInput();

        Button[] botones = panelAcertijo.GetComponentsInChildren<Button>(true);
        Button botonVerificar = null;
        Button botonCerrar = null;

        for (int i = 0; i < botones.Length; i++)
        {
            string texto = ObtenerTextoBoton(botones[i]).ToLowerInvariant();
            if (texto.Contains("verif"))
            {
                botonVerificar = botones[i];
            }
            else if (texto.Contains("cerr"))
            {
                botonCerrar = botones[i];
            }
        }

        if (botonVerificar != null)
        {
            botonVerificar.gameObject.SetActive(true);
            if (usarPosicionVerificarComoBano)
            {
                AjustarBotonDerechaInferior(botonVerificar, margenLateralVerificarComoBano, margenInferiorVerificarComoBano, tamanoBotonVerificar1080);
            }
            else
            {
                AjustarBoton(botonVerificar, posicionBotonVerificar1080, tamanoBotonVerificar1080);
            }
        }

        if (botonCerrar != null)
        {
            if (ocultarBotonCerrar)
            {
                botonCerrar.gameObject.SetActive(false);
            }
            else
            {
                botonCerrar.gameObject.SetActive(true);
                AjustarBoton(botonCerrar, posicionBotonCerrar1080, tamanoBotonCerrar1080);
            }
        }
    }

    private void AlinearContenidoInput()
    {
        if (cuadroDeTexto == null)
        {
            return;
        }

        RectTransform viewportRect = cuadroDeTexto.textViewport;
        if (viewportRect != null)
        {
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.pivot = new Vector2(0.5f, 0.5f);
            viewportRect.anchoredPosition = Vector2.zero;
            viewportRect.sizeDelta = new Vector2(-20f, -16f);
            viewportRect.localScale = Vector3.one;
        }

        if (cuadroDeTexto.textComponent != null)
        {
            RectTransform textRect = cuadroDeTexto.textComponent.rectTransform;
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.anchoredPosition = Vector2.zero;
                textRect.sizeDelta = Vector2.zero;
                textRect.localScale = Vector3.one;
            }

            cuadroDeTexto.textComponent.margin = new Vector4(10f, 8f, 10f, 8f);
            cuadroDeTexto.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
        }

        if (cuadroDeTexto.placeholder is TMP_Text placeholderTMP)
        {
            RectTransform placeholderRect = placeholderTMP.rectTransform;
            if (placeholderRect != null)
            {
                placeholderRect.anchorMin = Vector2.zero;
                placeholderRect.anchorMax = Vector2.one;
                placeholderRect.pivot = new Vector2(0.5f, 0.5f);
                placeholderRect.anchoredPosition = Vector2.zero;
                placeholderRect.sizeDelta = Vector2.zero;
                placeholderRect.localScale = Vector3.one;
            }

            placeholderTMP.margin = new Vector4(10f, 8f, 10f, 8f);
            placeholderTMP.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    private void AjustarBotonDerechaInferior(Button boton, float margenLateral, float margenInferior, Vector2 tamano)
    {
        RectTransform rect = boton.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-Mathf.Abs(margenLateral), Mathf.Abs(margenInferior));
            rect.sizeDelta = tamano;
            rect.localScale = Vector3.one;
        }

        boton.interactable = true;

        Image imagen = boton.GetComponent<Image>();
        if (imagen != null)
        {
            imagen.raycastTarget = true;
        }

        TMP_Text textoTMP = boton.GetComponentInChildren<TMP_Text>(true);
        if (textoTMP != null)
        {
            textoTMP.fontSize = tamanoFuenteBoton1080;
            textoTMP.raycastTarget = false;
        }
    }

    private void AjustarBoton(Button boton, Vector2 posicion, Vector2 tamano)
    {
        RectTransform rect = boton.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = posicion;
            rect.sizeDelta = tamano;
            rect.localScale = Vector3.one;
        }

        boton.interactable = true;

        Image imagen = boton.GetComponent<Image>();
        if (imagen != null)
        {
            imagen.raycastTarget = true;
        }

        TMP_Text textoTMP = boton.GetComponentInChildren<TMP_Text>(true);
        if (textoTMP != null)
        {
            textoTMP.fontSize = tamanoFuenteBoton1080;
            textoTMP.raycastTarget = false;
        }
    }

    private string ObtenerTextoBoton(Button boton)
    {
        TMP_Text textoTMP = boton.GetComponentInChildren<TMP_Text>(true);
        if (textoTMP != null && !string.IsNullOrWhiteSpace(textoTMP.text))
        {
            return textoTMP.text;
        }

        Text textoUGUI = boton.GetComponentInChildren<Text>(true);
        if (textoUGUI != null)
        {
            return textoUGUI.text;
        }

        return string.Empty;
    }

    private void AsegurarEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemGO = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystemGO);
    }

    private void ResolverHudMinijuego()
    {
        if (uiObjetosObligatorios == null)
        {
            uiObjetosObligatorios = GameObject.Find("ObjetosObligatorios");
        }

        if (uiEco == null)
        {
            uiEco = GameObject.Find("ECO");
            if (uiEco == null)
            {
                uiEco = GameObject.Find("ECO_Cocina");
            }
        }
    }

    private void AplicarVisibilidadHudMinijuego(bool visible)
    {
        if (uiObjetosObligatorios != null)
        {
            uiObjetosObligatorios.SetActive(visible);
        }

        if (uiEco != null)
        {
            uiEco.SetActive(visible);
        }
    }
}