using UnityEngine;

public class TutorialControles : MonoBehaviour
{
    [Header("UI de Controles")]
    [Tooltip("Arrastra aquí el Panel o Imagen que contiene los dibujos de las teclas")]
    public GameObject panelControles;

    void Start()
    {
        // Nos aseguramos de que el panel esté visible en cuanto empiece el nivel
        if (panelControles != null)
        {
            panelControles.SetActive(true);
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

            // Apagamos este mismo script para que Unity no siga revisando 
            // las teclas en cada frame y ahorremos memoria
            this.enabled = false; 
        }
    }
}