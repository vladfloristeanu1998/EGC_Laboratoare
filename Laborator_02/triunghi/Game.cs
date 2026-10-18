 using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Egc;

/// <summary>
/// Fereastra aplicației și bucla principală (main game loop).
/// GameWindow apelează automat metodele On... de mai jos, în ordinea descrisă în laborator.
/// </summary>
public class Game : GameWindow
{
    // Obiecte care NU depind de OpenGL pot fi create imediat.
    private readonly Camera _camera = new();

    // Obiectele care folosesc OpenGL se creează în OnLoad (abia atunci există contextul OpenGL).
    private Shader _shader = null!;
    private Triangle _triangle = null!;

    // Starea aplicației (se modifică în OnUpdateFrame, se folosește în OnRenderFrame).
    private Color4 _clearColor = new(0.0f, 0.0f, 0.3f, 1.0f);   // culoarea de fundal
    private Color4 _tint = Color4.White;                          // nuanța triunghiului (alb = neschimbat)
    private Vector3 _offset = Vector3.Zero;                       // deplasarea triunghiului

    // Contoare pentru exemplul cu bucla principală (afișate în titlul ferestrei).
    private readonly string _baseTitle;
    private double _elapsed;     // timpul total scurs (secunde)
    private double _secondTimer; // cronometru pentru actualizarea titlului o dată pe secundă
    private int _updates;        // câte OnUpdateFrame în ultima secundă
    private int _frames;         // câte OnRenderFrame în ultima secundă

    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default,
               new NativeWindowSettings
               {
                   ClientSize = new Vector2i(width, height),
                   Title = title,
                   APIVersion = new Version(3, 3),          // cerem OpenGL 3.3 ...
                   Profile = ContextProfile.Core,           // ... profil Core (fără modul imediat)
                   Flags = ContextFlags.ForwardCompatible
               })
    {
        _baseTitle = title;
        VSync = VSyncMode.On;   // swap-ul bufferelor așteaptă reîmprospătarea monitorului
    }

    /// <summary>Apelată O SINGURĂ DATĂ, după crearea ferestrei și a contextului OpenGL.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        // Informații despre driverul și placa grafică folosite (vezi Problema 1 din Laboratorul 01).
        Console.WriteLine($"OpenGL  : {GL.GetString(StringName.Version)}");
        Console.WriteLine($"Vendor  : {GL.GetString(StringName.Vendor)}");
        Console.WriteLine($"Renderer: {GL.GetString(StringName.Renderer)}");

        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

        _shader = Shader.FromFiles("Shaders/shader.vert", "Shaders/shader.frag");
        _triangle = new Triangle();
    }

    /// <summary>Apelată când se schimbă dimensiunea ferestrei.</summary>
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);   // zona ferestrei în care se desenează
        _camera.Resize(e.Width, e.Height);
    }

    /// <summary>Apelată la fiecare iterație a buclei: LOGICA aplicației (intrări, mișcare, timp).</summary>
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // args.Time = secundele scurse de la apelul anterior (delta time).
        _elapsed += args.Time;
        _updates++;

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        // --- TODO (P1–P5): codul vostru de logică se adaugă aici ---

        // Problema 1: Animația culorii de fundal
        float r = 0.5f + 0.5f * MathF.Sin((float)_elapsed);
        float g = 0.5f + 0.5f * MathF.Sin((float)_elapsed + 2.094f); // defazaj 2π/3
        float b = 0.5f + 0.5f * MathF.Sin((float)_elapsed + 4.189f); // defazaj 4π/3
        _clearColor = new Color4(r, g, b, 1.0f);

        // Problema 2b: Schimbarea nuanței folosind tastele 0, 1, 2, 3
        if (KeyboardState.IsKeyPressed(Keys.D1)) _tint = Color4.Red;
        if (KeyboardState.IsKeyPressed(Keys.D2)) _tint = Color4.Green;
        if (KeyboardState.IsKeyPressed(Keys.D3)) _tint = Color4.Blue;
        if (KeyboardState.IsKeyPressed(Keys.D0)) _tint = Color4.White;

        // Problema 2e: Schimbarea culorii unui singur vârf la rulare
        if (KeyboardState.IsKeyPressed(Keys.Space))
        {
            // Când apăsăm Space, setăm vârful de sus (index 2) la culoarea Alb
            _triangle.SetColor(2, 1.0f, 1.0f, 1.0f);
            // Este crucial să apelăm UpdateVertices pentru a trimite noile date din RAM pe placa grafică (VBO)
            _triangle.UpdateVertices();
        }

        // Exemplul bucla principală: o dată pe secundă afișăm în titlu câte
        // actualizări și câte cadre desenate au avut loc.
        _secondTimer += args.Time;
        if (_secondTimer >= 1.0)
        {
            Title = $"{_baseTitle} | update/s: {_updates} | render/s: {_frames}";
            _updates = 0;
            _frames = 0;
            _secondTimer -= 1.0;
        }
    }

    /// <summary>Apelată la fiecare iterație a buclei: DESENAREA unui cadru.</summary>
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        _frames++;

        // 1. Ștergem imaginea veche din back buffer. GL.ClearColor SETEAZĂ starea (culoarea de ștergere),
        //    GL.Clear o FOLOSEȘTE. Aici o setăm la fiecare cadru pentru că vom putea s-o schimbăm (P1).
        GL.ClearColor(_clearColor.R, _clearColor.G, _clearColor.B, _clearColor.A);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        // 2. Activăm programul shader și îi trimitem uniformele.
        _shader.Use();
        _shader.SetMatrix4("uView", _camera.ViewMatrix);
        _shader.SetMatrix4("uProjection", _camera.ProjectionMatrix);
        _shader.SetVector3("uOffset", _offset);
        _shader.SetColor("uTint", _tint);

        // 3. Desenăm triunghiul.
        _triangle.Draw();

        // 4. Afișăm cadrul terminat: back buffer <-> front buffer.
        SwapBuffers();
    }

    /// <summary>Apelată O SINGURĂ DATĂ, la închiderea ferestrei: eliberăm resursele GPU.</summary>
    protected override void OnUnload()
    {
        _triangle.Dispose();
        _shader.Dispose();

        base.OnUnload();
    }
}
