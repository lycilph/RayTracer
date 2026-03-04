namespace RayTracer;

internal class Program
{
    static void Main()
    {
        new Renderer().Render("output.ppm");

        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }
}
