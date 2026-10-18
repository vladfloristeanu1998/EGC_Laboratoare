using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Egc;

/// <summary>
/// Exemplul 1: cea mai simplă aplicație OpenTK – o fereastră al cărei fundal își schimbă culoarea în timp.
/// </summary>
public class Game : GameWindow
{
    private double _time;   // timpul total scurs, în secunde

    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default,
               new NativeWindowSettings
               {
                   ClientSize = new Vector2i(width, height),
                   Title = title,
                   APIVersion = new Version(3, 3),
                   Profile = ContextProfile.Core
               })
    {
    }

    // Se apelează O SINGURĂ DATĂ, după ce fereastra și contextul OpenGL au fost create.
    protected override void OnLoad()
    {
        base.OnLoad();
        Console.WriteLine($"OpenGL: {GL.GetString(StringName.Version)}");
    }

    // Se apelează la FIECARE iterație a buclei: logica aplicației.
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        _time += args.Time;                       // args.Time = secundele scurse de la apelul anterior

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();                              // cere închiderea ferestrei => bucla se oprește
        }
    }

    // Se apelează la FIECARE iterație a buclei: desenarea unui cadru.
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        float green = 0.5f + 0.5f * MathF.Sin((float)_time);   // oscilează între 0 și 1
        GL.ClearColor(0.2f, green, 0.3f, 1.0f);                // setează starea: culoarea de ștergere
        GL.Clear(ClearBufferMask.ColorBufferBit);              // folosește starea: șterge ecranul

        SwapBuffers();                                         // afișează cadrul terminat
    }
}
