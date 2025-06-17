namespace AAI.Core;

public static class StringHelper
{
    private static readonly List<string> StarWarsNames =
    [
        "Luke", "Leia", "Han", "Chewbacca", "Yoda", "Vader", "ObiWan", "Anakin",
        "Padme", "Rey", "Finn", "Poe", "Kylo", "Palpatine", "Mace", "Dooku", "Maul",
        "Tatooine", "Naboo", "Coruscant", "Hoth", "Endor", "Dagobah", "Mustafar", "Kamino",
        "Geonosis", "Alderaan", "Jakku", "Kashyyyk", "Bespin", "Scarif", "Dathomir",
        "Yavin 4", "Lothal", "Ilum", "Jedha", "Exegol"
    ];

    private static readonly Random Random = new Random();

    public static string GenerateUniqueName(int length = 6)
    {
        var starWarsName = StarWarsNames[Random.Next(StarWarsNames.Count)];
        var randomSuffix = GenerateRandomSuffix(length);
        return $"{starWarsName}-{randomSuffix}";
    }

    private static string GenerateRandomSuffix(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var suffix = new char[length];
        for (var currentIndex = 0; currentIndex < length; currentIndex++)
            suffix[currentIndex] = chars[Random.Next(chars.Length)];
        return new string(suffix);
    }
}