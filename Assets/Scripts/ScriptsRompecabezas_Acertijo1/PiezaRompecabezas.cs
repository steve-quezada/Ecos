using UnityEngine;
using UnityEngine.EventSystems;

public class PiezaRompecabezas : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configuración")]
    public int idPieza; // Un número para identificar si es la pieza 1, 2, 3...
    
    [HideInInspector] public bool yaEncajada = false;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 posicionInicial;
    private Canvas canvasPrincipal;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasPrincipal = GetComponentInParent<Canvas>();
        
        // El CanvasGroup nos sirve para que la pieza deje pasar el clic a través de ella
        // cuando la estamos arrastrando, así el Hueco puede detectar que llegó.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (yaEncajada) return; // Si ya está en su lugar, no dejamos que la mueva

        posicionInicial = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f; // La hacemos un poco transparente al arrastrar
        canvasGroup.blocksRaycasts = false; // Desactivamos su colisión para que el Hueco la detecte
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (yaEncajada) return;
        
        // Movemos la pieza junto con el mouse, adaptado a la escala de la pantalla
        rectTransform.anchoredPosition += eventData.delta / canvasPrincipal.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (yaEncajada) return;

        canvasGroup.alpha = 1f; // Regresa a su color normal
        canvasGroup.blocksRaycasts = true; // Vuelve a ser sólida

        // Si la soltamos y el Hueco NO le cambió el estado a "yaEncajada", 
        // significa que la soltamos en el lugar equivocado, así que regresa al inicio.
        if (!yaEncajada)
        {
            rectTransform.anchoredPosition = posicionInicial;
        }
    }
}