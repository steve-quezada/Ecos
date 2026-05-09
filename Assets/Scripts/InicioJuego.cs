using UnityEngine;

public class InicioJuego : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject guitarraPrefab;
    public Transform posicionCama; // Un Empty Object sobre la cama
    public MiloController milo;

    private GameObject guitarraInstanciada;
    private bool miloEnCama = true;

    void Start()
    {
        // 1. Posicionamos a Milo en la cama y bloqueamos su movimiento
        milo.transform.position = posicionCama.position;
        milo.puedeMoverse = false;

        // 2. Instanciamos la guitarra sobre él
        // La ponemos un poco más arriba (0.5f en Y) para que se vea encima
        Vector3 posicionGuitarra = posicionCama.position + new Vector3(0, 0.5f, 0);
        guitarraInstanciada = Instantiate(guitarraPrefab, posicionGuitarra, Quaternion.identity);
        
        // Desactivamos la física de la guitarra al inicio para que no se caiga sola
        Rigidbody2D rbGuitarra = guitarraInstanciada.GetComponent<Rigidbody2D>();
        if (rbGuitarra != null) rbGuitarra.simulated = false;

        Debug.Log("Milo está durmiendo con su guitarra...");
    }

    void Update()
    {
        // 3. Detectamos cuando Milo se levanta (puedes usar 'E' o cualquier tecla de movimiento)
        if (miloEnCama && (Input.GetKeyDown(KeyCode.E) || Input.GetAxis("Horizontal") != 0))
        {
            LevantarseDeLaCama();
        }
    }

    void LevantarseDeLaCama()
    {
        miloEnCama = false;
        milo.puedeMoverse = true;

        // 4. La guitarra se cae
        Rigidbody2D rbGuitarra = guitarraInstanciada.GetComponent<Rigidbody2D>();
        if (rbGuitarra != null)
        {
            rbGuitarra.simulated = true;
            
            // Le damos un empujón microscópico solo para que se despegue, o lo puedes borrar por completo
            rbGuitarra.AddForce(new Vector2(0.1f, 0f), ForceMode2D.Impulse); 
        }

        // 5. ¡EL ESTRUENDO! Subimos el ruido en 5 de jalón
        if (ManagerRuido.instancia != null)
        {
            ManagerRuido.instancia.AgregarRuido(5f);
        }

        Debug.Log("¡ESTRUENDO! La guitarra cayó. El ruido subió a 5.");
        
        // Desactivamos este script para que no se repita
        this.enabled = false;
    }
}