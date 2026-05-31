using UnityEngine;
using UnityEngine.UI;

public class TutorialControles : MonoBehaviour
{
    [Header("UI de Controles")]
    [Tooltip("Arrastra aquí el Panel o Imagen que contiene los dibujos de las teclas")]
    public GameObject panelControles;

    [Header("Oscurecimiento del Tutorial")]
    public bool crearFondoOscuroAutomatico = true;
    [Range(0f, 1f)] public float opacidadFondoOscuro = 0.42f;
    public Color colorFondoOscuro = Color.black;

    private GameObject fondoOscuroAuto;

    void Start()
    {
        // Nos aseguramos de que el panel esté visible en cuanto empiece el nivel
        if (panelControles != null)
        {
            panelControles.SetActive(true);
            ConfigurarFondoOscuro();
        }
    }

    void Update()
    {
        // Si el jugador presiona CUALQUIER tecla o hace clic...
        if (Input.anyKeyDown)
        {
            // Apagamos los controles
            if (panelControles != null)
            {
                panelControles.SetActive(false);
            }

            if (fondoOscuroAuto != null)
            {
                fondoOscuroAuto.SetActive(false);
            }

            // Apagamos este mismo script para que Unity no siga revisando 
            // las teclas en cada frame y ahorremos memoria
            this.enabled = false; 
        }
    }

    void ConfigurarFondoOscuro()
    {
        if (!crearFondoOscuroAutomatico || panelControles == null)
        {
            return;
        }

        RectTransform panelRect = panelControles.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            return;
        }

        Transform existente = panelRect.Find("FondoOscuroAuto");
        if (existente != null)
        {
            fondoOscuroAuto = existente.gameObject;
        }
        else
        {
            fondoOscuroAuto = new GameObject("FondoOscuroAuto", typeof(RectTransform), typeof(Image));
            fondoOscuroAuto.transform.SetParent(panelRect, false);

            RectTransform rect = fondoOscuroAuto.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        Image imagenFondo = fondoOscuroAuto.GetComponent<Image>();
        if (imagenFondo != null)
        {
            Color color = colorFondoOscuro;
            float opacidadAplicada = Mathf.Min(Mathf.Clamp01(opacidadFondoOscuro), 0.45f);
            color.a = opacidadAplicada;
            imagenFondo.color = color;
            imagenFondo.raycastTarget = false;
        }

        fondoOscuroAuto.transform.SetAsFirstSibling();
        fondoOscuroAuto.SetActive(true);
    }
}