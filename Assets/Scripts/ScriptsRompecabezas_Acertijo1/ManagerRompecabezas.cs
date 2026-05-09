using UnityEngine;

public class ManagerRompecabezas : MonoBehaviour
{
    public int totalPiezas; // Cuántas piezas hay en total
    private int piezasEncajadas = 0;
    
    public EcoInteractuable ecoOso; // Para poder avisarle al oso que ya ganamos

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
        ecoOso.CerrarRompecabezas();
        
        // Destruimos al oso (o lo apagamos) para que ya no se pueda interactuar con él
        Destroy(ecoOso.gameObject); 
    }
}