using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiadorEscenaSimple : MonoBehaviour
{
    public void IrAlInicio()
    {
        // Cargamos la escena 0 (que es tu Menú Principal)
        SceneManager.LoadScene(0);
    }
}