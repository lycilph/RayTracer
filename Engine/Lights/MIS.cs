namespace Engine.Lights;

public static class MIS
{
    // Balance heuristic — optimal MIS weighting for two sampling strategies.
    // pdfA = PDF of the strategy that generated this sample
    // pdfB = PDF of the other strategy for the same sample direction
    //
    // Returns the weight to apply to this sample's contribution.
    // The two weights always sum to 1: Weight(a,b) + Weight(b,a) = 1
    public static double BalanceWeight(double pdfA, double pdfB)
    {
        // Guard against both PDFs being zero — no valid sample
        double sum = pdfA + pdfB;
        return sum > 0 ? pdfA / sum : 0;
    }

    // Power heuristic — Veach's recommended variant (β = 2).
    // Gives lower variance than the balance heuristic when one strategy
    // is strongly dominant. The balance heuristic is a special case with β = 1.
    // We expose both so you can compare them on the same scene.
    public static double PowerWeight(double pdfA, double pdfB)
    {
        double a2 = pdfA * pdfA;
        double b2 = pdfB * pdfB;
        double sum = a2 + b2;
        return sum > 0 ? a2 / sum : 0;
    }
}