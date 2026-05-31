using UnityEngine;
using System.Collections;

public class ManagerRompecabezas : MonoBehaviour
{
    [Header("Elementos de la UI a ocultar")]
    public GameObject uiObjetosObligatorios;
    public GameObject uiEco;

    [Header("Configuración del Acertijo")]
    public int totalPiezas; // Cuántas piezas hay en total
    private int piezasEncajadas = 0;

    [Header("Visibilidad de Piezas (1080p)")]
    [SerializeField] private bool forzarPiezasDentroAreaVisible = true;
    [SerializeField] private RectTransform areaVisiblePiezas;
    [SerializeField] private Vector2 margenAreaVisible = new Vector2(24f, 24f);
    
    public EcoInteractuable ecoOso; // Para poder avisarle al oso que ya ganamos
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // OnEnable se ejecuta automáticamente en cuanto el Panel de este script se ACTIVA
    void OnEnable()
    {
        AsegurarPanelPantallaCompleta();

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.OcultarHudProgresoEnMinijuego();
        }

        ResolverHudMinijuego();
        if (uiObjetosObligatorios != null) uiObjetosObligatorios.SetActive(false);
        if (uiEco != null) uiEco.SetActive(false);
        AjustarPiezasDentroDeAreaVisible();
        StartCoroutine(AjustarPiezasSiguienteFrame());
    }

    // OnDisable se ejecuta automáticamente en cuanto el Panel de este script se DESACTIVA (lo termines o no)
    void OnDisable()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.RestaurarHudProgresoTrasMinijuego();
        }

        if (uiObjetosObligatorios != null) uiObjetosObligatorios.SetActive(true);
        if (uiEco != null) uiEco.SetActive(true);
    }

    public void AnotarAcierto()
    {
        piezasEncajadas++;
        
        if (piezasEncajadas >= totalPiezas)
        {
            Debug.Log("¡ROMPECABEZAS RESUELTO!");
            CompletarAcertijo();
        }
    }

    void CompletarAcertijo()
    {
        if (gameManager != null)
        {
            gameManager.RegistrarEcoRecolectado();
        }

        // Aquí podrías agregarle un objeto al inventario de Milo (ej. una llave secreta que tenía el oso)
        
        // Cerramos el panel y descongelamos el juego
        ecoOso.CerrarRompecabezas(); // ¡Al hacer esto se dispara el OnDisable automáticamente!
        
        // Destruimos al oso (o lo apagamos) para que ya no se pueda interactuar con él
        Destroy(ecoOso.gameObject); 
    }

    private void AjustarPiezasDentroDeAreaVisible()
    {
        if (!forzarPiezasDentroAreaVisible)
        {
            return;
        }

        RectTransform area = ObtenerAreaVisible();
        if (area == null)
        {
            Debug.LogWarning("ManagerRompecabezas: no se encontró área visible para limitar piezas.");
            return;
        }

        PiezaRompecabezas[] piezas = GetComponentsInChildren<PiezaRompecabezas>(true);
        for (int i = 0; i < piezas.Length; i++)
        {
            RectTransform rectPieza = piezas[i].GetComponent<RectTransform>();
            if (rectPieza == null)
            {
                continue;
            }

            ClampearRectTransformDentroDeArea(rectPieza, area);
        }
    }

    private RectTransform ObtenerAreaVisible()
    {
        if (areaVisiblePiezas != null)
        {
            return areaVisiblePiezas;
        }

        return transform as RectTransform;
    }

    private void ClampearRectTransformDentroDeArea(RectTransform pieza, RectTransform area)
    {
        Vector3[] esquinasArea = new Vector3[4];
        Vector3[] esquinasPieza = new Vector3[4];

        area.GetWorldCorners(esquinasArea);
        pieza.GetWorldCorners(esquinasPieza);

        float minX = esquinasArea[0].x + margenAreaVisible.x;
        float maxX = esquinasArea[2].x - margenAreaVisible.x;
        float minY = esquinasArea[0].y + margenAreaVisible.y;
        float maxY = esquinasArea[2].y - margenAreaVisible.y;

        float deltaX = 0f;
        float deltaY = 0f;

        if (esquinasPieza[0].x < minX)
        {
            deltaX = minX - esquinasPieza[0].x;
        }
        else if (esquinasPieza[2].x > maxX)
        {
            deltaX = maxX - esquinasPieza[2].x;
        }

        if (esquinasPieza[0].y < minY)
        {
            deltaY = minY - esquinasPieza[0].y;
        }
        else if (esquinasPieza[2].y > maxY)
        {
            deltaY = maxY - esquinasPieza[2].y;
        }

        if (deltaX != 0f || deltaY != 0f)
        {
            pieza.position += new Vector3(deltaX, deltaY, 0f);
        }
    }

    private IEnumerator AjustarPiezasSiguienteFrame()
    {
        yield return null;
        AjustarPiezasDentroDeAreaVisible();
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

    private void AsegurarPanelPantallaCompleta()
    {
        RectTransform rectPanel = transform as RectTransform;
        if (rectPanel == null)
        {
            return;
        }

        rectPanel.anchorMin = Vector2.zero;
        rectPanel.anchorMax = Vector2.one;
        rectPanel.offsetMin = Vector2.zero;
        rectPanel.offsetMax = Vector2.zero;
        rectPanel.localScale = Vector3.one;

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        transform.SetAsLastSibling();
    }
}