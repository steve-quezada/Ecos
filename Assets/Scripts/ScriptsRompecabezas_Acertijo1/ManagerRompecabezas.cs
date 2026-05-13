using UnityEngine;

public class ManagerRompecabezas : MonoBehaviour
{
    [Header("Elementos de la UI a ocultar")]
    public GameObject uiObjetosObligatorios;
    public GameObject uiEco;

    [Header("Configuración del Acertijo")]
    public int totalPiezas; // Cuántas piezas hay en total
    private int piezasEncajadas = 0;
    
    public EcoInteractuable ecoOso; // Para poder avisarle al oso que ya ganamos

    // OnEnable se ejecuta automáticamente en cuanto el Panel de este script se ACTIVA
    void OnEnable()
    {
        if (uiObjetosObligatorios != null) uiObjetosObligatorios.SetActive(false);
        if (uiEco != null) uiEco.SetActive(false);
    }

    // OnDisable se ejecuta automáticamente en cuanto el Panel de este script se DESACTIVA (lo termines o no)
    void OnDisable()
    {
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
        // Aquí podrías agregarle un objeto al inventario de Milo (ej. una llave secreta que tenía el oso)
        
        // Cerramos el panel y descongelamos el juego
        ecoOso.CerrarRompecabezas(); // ¡Al hacer esto se dispara el OnDisable automáticamente!
        
        // Destruimos al oso (o lo apagamos) para que ya no se pueda interactuar con él
        Destroy(ecoOso.gameObject); 
    }
}