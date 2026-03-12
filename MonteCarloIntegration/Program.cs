namespace MonteCarloIntegration;

internal class Program
{
    static Random rng = new(); // new(42); // Fixed seed so results are reproducible

    static double F(double x) => x * x;
    const double ExactAnswer = 1.0 / 3.0;

    static void Main(string[] args)
    {
        BasicEstimator();
        ConvergenceRate();
        VarianceAcrossRuns();
        NonUniformP();
        CompareVariance();


       Console.WriteLine("Done. Press any key to exit.");
        Console.ReadKey();
    }

    private static void BasicEstimator()
    {
        Console.WriteLine("Basic Estimator example");

        int samples = 100;
        double result = 0;

        for (int i = 0; i < samples; i++)
        {
            var x = rng.NextDouble();
            result += F(x);
        }

        result = result / samples;

        Console.WriteLine($"Result = {result}, with {samples} samples");
    }

    private static void ConvergenceRate()
    {
        Console.WriteLine("Convergence rate example");

        foreach (var samples in new[] { 10, 100, 1_000, 10_000, 100_000, 1000_000, 10_000_000 })
        {
            double result = 0;

            for (int i = 0; i < samples; i++)
            {
                var x = rng.NextDouble();
                result += F(x);
            }

            result = result / samples;
            double error = Math.Abs(result - ExactAnswer);
            double sqrt_x_times_error = Math.Sqrt(samples) * error;

            Console.WriteLine($"Result = {result}, Error = {error}, sqrt(n)*error = {sqrt_x_times_error}, with {samples} samples");
        }
    }

    private static void VarianceAcrossRuns()
    {
        var runs = 10;
        var samples = 100;

        Console.WriteLine($"Variance across runs example ({runs} runs with {samples} samples)");

        for (int run = 0; run < runs; run++)
        {
            double result = 0;

            for (int i = 0; i < samples; i++)
            {
                var x = rng.NextDouble();
                result += F(x);
            }

            result = result / samples;
            Console.WriteLine($"Result = {result}, run {run}");

        }

    }

    private static void NonUniformP()
    {
        Console.WriteLine("Non-uniform p(x)");

        int samples = 1000;
        double result = 0;

        for (int i = 0; i < samples; i++)
        {
            var u = rng.NextDouble();
            var x = Math.Sqrt(u);

            var f = F(x);
            var p = 2 * x;

            result += f/p;
        }

        result = result / samples;

        Console.WriteLine($"Result = {result}, with {samples} samples");
    }

    private static void CompareVariance()
    {
        Console.WriteLine("Compare Variance for uniform vs non-uniform p");

        foreach (var samples in new[] { 10, 100, 1_000, 10_000, 100_000, 1000_000, 10_000_000 })
        {
            double uniform_result = 0;
            double nonuniform_result = 0;

            for (int i = 0; i < samples; i++)
            {
                var x = rng.NextDouble();
                uniform_result += F(x);
            }

            uniform_result = uniform_result / samples;
            double uniform_error = Math.Abs(uniform_result - ExactAnswer);

            for (int i = 0; i < samples; i++)
            {
                var u = rng.NextDouble();
                var x = Math.Sqrt(u);

                var f = F(x);
                var p = 2 * x;

                nonuniform_result += f / p;
            }

            nonuniform_result = nonuniform_result / samples;
            double nonuniform_error = Math.Abs(nonuniform_result - ExactAnswer);


            Console.WriteLine($"Uniform = {uniform_result:f4}, Error = {uniform_error:f4} - Nonuniform = {nonuniform_result:f4}, Error = {nonuniform_error:f4}, with {samples} samples");
        }
    }
}