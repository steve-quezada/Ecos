using UnityEngine;
using UnityEngine.SceneManagement; // Crucial para viajar entre niveles

public class MenuPrincipal : MonoBehaviour
{
    [Header("Ventanas")]
    public GameObject panelReglas; // Arrastraremos tu imagen de reglas aquí

    [Header("Fondo de Reglas")]
    public GameObject fondoMenu;
    public Camera camaraMenu;

    private CameraClearFlags clearFlagsOriginal;
    private Color colorFondoOriginal;
    private bool camaraInicializada = false;

    void Start()
    {
        if (camaraMenu == null)
        {
            camaraMenu = Camera.main;
        }

        if (fondoMenu == null)
        {
            GameObject posibleFondo = GameObject.Find("FondoMenu");
            if (posibleFondo != null)
            {
                fondoMenu = posibleFondo;
            }
        }

        if (camaraMenu != null)
        {
            clearFlagsOriginal = camaraMenu.clearFlags;
            colorFondoOriginal = camaraMenu.backgroundColor;
            camaraInicializada = true;
        }

        // Por seguridad, nos aseguramos de que las reglas empiecen apagadas
        if (panelReglas != null)
        {
            panelReglas.SetActive(false);
        }

        if (fondoMenu != null)
        {
            fondoMenu.SetActive(true);
        }

        RestaurarFondoCamara();
    }

    public void EmpezarJuego()
    {
        // IMPORTANTE: Cambia "HabitacionMilo" por el nombre exacto de tu primera escena
        SceneManager.LoadScene("HabitacionMilo");
    }

    public void AbrirReglas()
    {
        if (panelReglas != null)
        {
            panelReglas.SetActive(true);
        }

        if (fondoMenu != null)
        {
            fondoMenu.SetActive(false);
        }

        if (camaraMenu != null)
        {
            camaraMenu.clearFlags = CameraClearFlags.SolidColor;
            camaraMenu.backgroundColor = Color.black;
        }
    }

    public void CerrarReglas()
    {
        if (panelReglas != null)
        {
            panelReglas.SetActive(false);
        }

        if (fondoMenu != null)
        {
            fondoMenu.SetActive(true);
        }

        RestaurarFondoCamara();
    }

    public void SalirDelJuego()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit(); // Ojo: Esto solo cierra el juego cuando ya está exportado (Build), en el editor de Unity no hace nada visualmente.
    }

    private void RestaurarFondoCamara()
    {
        if (camaraMenu == null || !camaraInicializada)
        {
            return;
        }

        camaraMenu.clearFlags = clearFlagsOriginal;
        camaraMenu.backgroundColor = colorFondoOriginal;
    }
}