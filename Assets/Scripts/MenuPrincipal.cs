using UnityEngine;
using UnityEngine.SceneManagement; // Crucial para viajar entre niveles

public class MenuPrincipal : MonoBehaviour
{
    [Header("Ventanas")]
    public GameObject panelReglas; // Arrastraremos tu imagen de reglas aquí

    void Start()
    {
        // Por seguridad, nos aseguramos de que las reglas empiecen apagadas
        if (panelReglas != null)
        {
            panelReglas.SetActive(false);
        }
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
    }

    public void CerrarReglas()
    {
        if (panelReglas != null)
        {
            panelReglas.SetActive(false);
        }
    }

    public void SalirDelJuego()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit(); // Ojo: Esto solo cierra el juego cuando ya está exportado (Build), en el editor de Unity no hace nada visualmente.
    }
}