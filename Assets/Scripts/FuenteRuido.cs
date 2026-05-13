using UnityEngine;

public class FuenteRuido : MonoBehaviour
{
    public int puntosDeRuido = 1;
    private GameManager gameManager;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        if (ManagerRuido.instancia != null)
        {
            ManagerRuido.instancia.AgregarRuido(puntosDeRuido);
            return;
        }

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.AgregarRuido(puntosDeRuido);
        }
        else
        {
            Debug.LogWarning("No existe gestor de ruido en la escena para: " + gameObject.name);
        }
    }
}