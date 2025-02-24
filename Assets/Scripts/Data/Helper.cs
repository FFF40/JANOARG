public static class Helper 
{
    public static string GetRank(float score)
    {
        if (score >= 1000000) return "1";
        if (score >=  990000) return "SSS";
        if (score >=  975000) return "SS";
        if (score >=  960000) return "S";
        if (score >=  940000) return "AAA";
        if (score >=  925000) return "AA";
        if (score >=  900000) return "A";
        if (score >=  800000) return "B";
        if (score >=  700000) return "C";
        if (score >=       1) return "D";
        return "?";
    }
}