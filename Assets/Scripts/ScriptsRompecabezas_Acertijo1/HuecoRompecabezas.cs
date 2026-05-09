using UnityEngine;
using UnityEngine.EventSystems;

public class HuecoRompecabezas : MonoBehaviour, IDropHandler
{
    public int idHueco; // Debe coincidir con el idPieza de la pieza correcta
    private ManagerRompecabezas manager;

    void Start()
    {
        manager = FindObjectOfType<ManagerRompecabezas>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Alguien soltó algo encima del hueco: " + idHueco);
        // Si soltamos algo que se puede arrastrar...
        if (eventData.pointerDrag != null)
        {
            PiezaRompecabezas pieza = eventData.pointerDrag.GetComponent<PiezaRompecabezas>();

            // Si es una pieza, tiene el mismo ID que este hueco, y no ha sido encajada aún...
            if (pieza != null && pieza.idPieza == idHueco && !pieza.yaEncajada)
            {
                // ¡Acierto! Colocamos la pieza exactamente en el centro del hueco
                pieza.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
                
                // La bloqueamos para que ya no la puedan mover
                pieza.yaEncajada = true;
                
                // Le avisamos al Manager que tenemos un acierto
                manager.AnotarAcierto();
                
                Debug.Log("Pieza " + idHueco + " colocada correctamente.");
            }
        }
    }
}