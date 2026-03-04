using System.Diagnostics;

namespace RayTracer;

internal class Program
{
    static void Main()
    {
        var sw = Stopwatch.StartNew();

        new Renderer().Render("output.ppm");

        sw.Stop();
        Console.WriteLine("Time elapsed: " + sw.Elapsed);

        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }
}
