using System;
using System.Collections.Generic;
using CSScriptLibrary;

namespace ScriptingTest
{
    public interface ISceneBuilder
    {
        void CreateScene(Scene scene);
    }

    public class Scene
    {
        public List<Shape> shapes = new List<Shape>();
    }

    public class Shape
    {
        public string Name;
    }
    
    class Program
    {
        //void CreateScene(Scene scene)
        //{
        //    var s1 = new Shape() { Name = "Circle" };
        //    var s2 = new Shape() { Name = "Triangle" };
        //    scene.shapes.Add(s1);
        //    scene.shapes.Add(s2);
        //}

        //using ScriptingTest; 
        //public class SceneBuilder : ISceneBuilder 
        // {
        // [Script content]
        // }

        static void Main(string[] args)
        {
            CSScript.EvaluatorConfig.Engine = EvaluatorEngine.CodeDom;

            var script_text = "public void CreateScene(Scene scene) { var s1 = new Shape() { Name = \"Circle\" }; var s2 = new Shape() { Name = \"Triangle\" }; scene.shapes.Add(s1); scene.shapes.Add(s2); }";
            var full_script_text = $"using ScriptingTest; public class SceneBuilder : ISceneBuilder {{ {script_text} }}";

            try
            {
                dynamic script = CSScript.Evaluator.LoadCode(full_script_text);
                var scene = new Scene();
                script.CreateScene(scene);

                Console.WriteLine($"Found {scene.shapes.Count} shapes");
                foreach (var shape in scene.shapes)
                    Console.WriteLine($"Found shape: {shape.Name}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
