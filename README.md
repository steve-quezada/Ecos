<div align="center">
  <img width="200" src="https://www.fciencias.unam.mx/sites/default/files/logoFC_2.png" alt="Logo FC">
  <h1>Diseño y Programación de Videojuegos</h1>
  <p>
    <strong>Profesor:</strong> Enrique Ehecatl Hernández Ferreiro <br>
    <strong>Ayudante:</strong> Joel Haidd Reyes Cedillo <br>
    <strong>Ayud. Lab.:</strong> Jesús Haans López Hérnandez <br>
    <br>
    <strong>Osos de peluche:</strong><br>
    &emsp;Camacho Gutiérrez Karla Alejandra<br>
    &emsp;Cruz Campos José Eduardo<br>
    &emsp;Quezada Ordoñez Kevin Steve<br>
    &emsp;Trujillo Beltrán Zianya Nenetzi<br>
  </p>
</div>




---

<div align="center">
    <h2>ECOS</h2>
</div>

### Estructura del proyecto [**Unity 2022.3.62f3**]
<div align="center">
</div>

```text
Proyecto-Final [Ecos]/                            ← Carpeta raíz del proyecto
│
├── .gitattributes                                ← Atributos Git
├── .gitignore                                    ← Ignorados Git
│
├── README.md                                     ← Este archivo
│
├── Assets/                                       ← Contenido principal del juego (escenas, scripts, arte, audio)
│   ├── AssetsAcertijos/                          ←     Recursos para minijuegos y acertijos
│   │   └── Acertijo1_Rompecabezas/               ←         Elementos del acertijo 1 (rompecabezas)
│   │       ├── MarcoRompecabezas.png             ←             Marco visual
│   │       ├── Oso_1.png ... Oso_9.png           ←             Piezas del rompecabezas
│   │       └── square.jpeg                       ←             Textura/base de apoyo
│   │
│   ├── Characters/                               ← Sprites de personajes
│   │   ├── Estatica.png                          
│   │   └── Milo.png                              
│   │
│   ├── Furniture/                                ← Arte de escenario y objetos interactuables
│   │   ├── Piso.png                              
│   │   │
│   │   └── Recamara/                             ←     Objetos visuales de la recámara
│   │       ├── Bocina.png                        
│   │       ├── Cajonera.png                      
│   │       ├── Cama.png                          
│   │       ├── Closet.png                        
│   │       ├── Escritorio.png                    
│   │       ├── Guitarra.png                      
│   │       ├── Guitarra.prefab                   
│   │       ├── LLave.png                         
│   │       ├── Librero.png                       
│   │       ├── Mochila.png                       
│   │       ├── Oso_Eco.png                       
│   │       ├── VentanaRota.png                   
│   │       ├── Vidrios_Rotos.png                 
│   │       └── ZapatosTirados.png                
│   │
│   ├── Otros/                                    ← Configuración de render y pipeline
│   │   ├── ConfiguracionLuces.asset              
│   │   ├── ConfiguracionLuces_Renderer.asset     
│   │   └── UniversalRenderPipeline.asset         
│   │
│   ├── Scenes/                                   ← Escenas jugables de la casa
│   │   ├── Baño.unity                            
│   │   ├── Cocina.unity                          
│   │   ├── HabitaciónMilo.unity                  
│   │   └── Sala.unity                            
│   │
│   ├── Scripts/                                  ← Lógica de juego en C#
│   │   ├── CambioHabitación.cs                   ←     Control de transición entre habitaciones
│   │   ├── EcoInteractuable.cs                   ←     Interacción con objetos tipo Eco
│   │   ├── Escondite.cs                          ←     Mecánica de esconderse
│   │   ├── EstaticaController.cs                 ←     Comportamiento de la entidad Estática
│   │   ├── FuenteRuido.cs                        ←     Emisores de ruido en el mapa
│   │   ├── GameManager.cs                        ←     Coordinación general del juego
│   │   ├── InicioJuego.cs                        ←     Flujo y arranque de partida
│   │   ├── ManagerRuido.cs                       ←     Gestión central de eventos de ruido
│   │   ├── MiloController.cs                     ←     Movimiento y control de Milo
│   │   ├── ObjetoObligatorio.cs                  ←     Lógica para objetivos obligatorios
│   │   ├── PuertaSalida.cs                       ←     Condiciones de salida/nivel
│   │   ├── SonidoCaida.cs                        ←     Reproducción de sonido por caída
│   │   ├── TrampaRuido.cs                        ←     Objetos que activan ruido/trampas
│   │   │   
│   │   └── ScriptsRompecabezas_Acertijo1/        ←     Lógica específica del acertijo 1
│   │       ├── HuecoRompecabezas.cs              ←         Control de huecos receptores
│   │       ├── ManagerRompecabezas.cs            ←         Estado y validación del rompecabezas
│   │       └── PiezaRompecabezas.cs              ←         Comportamiento de piezas movibles
│   │
│   ├── Sonidos/                                  ← Recursos de audio del juego
│   │   ├── CaidaGuitarra.mp3                     ←     Sonido de caída de guitarra
│   │   ├── GritoPerdedor.mp3                     ←     Sonido al perder la partida
│   │   ├── Radio.mp3                             ←     Ambiente/objeto radio
│   │   ├── Tenis.mp3                             ←     Efecto asociado a tenis
│   │   └── Vidrios.mp3                           ←     Efecto de vidrios rotos
│   │
│   └── TextMesh Pro/                             ← Recursos internos de texto y fuentes TMP
│       ├── Documentation/                        
│       ├── Fonts/                                
│       ├── Resources/                            
│       ├── Shaders/                              
│       └── Sprites/                              
│
├── Packages/                                     ← Dependencias instaladas vía Package Manager
│   ├── manifest.json                             
│   └── packages-lock.json                        
│
└── ProjectSettings/                              ← Configuración global del proyecto Unity
  ├── AudioManager.asset                          
  ├── ClusterInputManager.asset                   
  ├── DynamicsManager.asset                       
  ├── EditorBuildSettings.asset                   
  ├── EditorSettings.asset                        
  ├── GraphicsSettings.asset                      
  ├── InputManager.asset                          
  ├── MemorySettings.asset                        
  ├── NavMeshAreas.asset                          
  ├── NetworkManager.asset                        
  ├── PackageManagerSettings.asset                
  ├── Physics2DSettings.asset                     
  ├── PresetManager.asset                         
  ├── ProjectSettings.asset                       
  ├── ProjectVersion.txt                          
  ├── QualitySettings.asset                       
  ├── SceneTemplateSettings.json                  
  ├── TagManager.asset                            
  ├── TimeManager.asset                           
  ├── UnityConnectSettings.asset                  
  ├── VFXManager.asset                            
  ├── VersionControlSettings.asset                
  └── XRSettings.asset                            
``` 

