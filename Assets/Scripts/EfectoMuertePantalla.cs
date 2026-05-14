using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EfectoMuertePantalla : MonoBehaviour
{
    private static EfectoMuertePantalla instancia;

    private Image panelNegro;

    public static IEnumerator FundirANegro(float duracionSegundos)
    {
        if (duracionSegundos <= 0f)
        {
            duracionSegundos = 0.01f;
        }

        AsegurarInstancia();
        return instancia.CorutinaFundido(duracionSegundos);
    }

    private static void AsegurarInstancia()
    {
        if (instancia != null)
        {
            return;
        }

        GameObject contenedor = new GameObject("EfectoMuertePantalla");
        instancia = contenedor.AddComponent<EfectoMuertePantalla>();
        instancia.CrearOverlay();
    }

    private void CrearOverlay()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        gameObject.AddComponent<CanvasScaler>();

        GameObject panel = new GameObject("PanelNegro");
        panel.transform.SetParent(transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        panelNegro = panel.AddComponent<Image>();
        panelNegro.raycastTarget = false;
        panelNegro.color = new Color(0f, 0f, 0f, 0f);
    }

    private IEnumerator CorutinaFundido(float duracionSegundos)
    {
        if (panelNegro == null)
        {
            CrearOverlay();
        }

        float tiempo = 0f;
        Color colorPanel = panelNegro.color;
        colorPanel.a = 0f;
        panelNegro.color = colorPanel;

        while (tiempo < duracionSegundos)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionSegundos);
            colorPanel.a = t;
            panelNegro.color = colorPanel;
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (instancia == this)
        {
            instancia = null;
        }
    }
}