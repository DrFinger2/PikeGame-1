using System.Text;

[System.Serializable]
public class WetlandDayResolution
{
    public WeatherType weather;

    public int waterDelta;
    public int oxygenDelta;
    public int plantBalanceDelta;
    public int animalBalanceDelta;
    public int stabilityDelta;

    public int invasiveDelta;
    public int coinsDelta;

    public bool firstMilestoneReached;
    public bool firstPikeMilestoneReached;
    public bool wonThisDay;
    public bool lostThisDay;

    public string GetSummary(int dayNumber, int stabilityValue)
    {
        StringBuilder builder = new StringBuilder(160);
        builder.Append("Day ").Append(dayNumber).Append(" resolved. ");
        builder.Append("Weather: ").Append(weather).Append(". ");
        builder.Append("Water ").Append(FormatDelta(waterDelta)).Append(", ");
        builder.Append("Oxygen ").Append(FormatDelta(oxygenDelta)).Append(", ");
        builder.Append("Plant ").Append(FormatDelta(plantBalanceDelta)).Append(", ");
        builder.Append("Animal ").Append(FormatDelta(animalBalanceDelta)).Append(", ");
        builder.Append("Stability ").Append(FormatDelta(stabilityDelta)).Append(" => ").Append(stabilityValue).Append(".");

        if (coinsDelta != 0)
        {
            builder.Append(" Coins ").Append(FormatDelta(coinsDelta)).Append('.');
        }

        if (firstMilestoneReached)
        {
            builder.Append(" First milestone reached.");
        }

        if (firstPikeMilestoneReached)
        {
            builder.Append(" First pike milestone reached.");
        }

        if (wonThisDay)
        {
            builder.Append(" Victory condition achieved.");
        }

        if (lostThisDay)
        {
            builder.Append(" Defeat condition achieved.");
        }

        return builder.ToString();
    }

    private static string FormatDelta(int value)
    {
        return value >= 0 ? "+" + value : value.ToString();
    }
}
