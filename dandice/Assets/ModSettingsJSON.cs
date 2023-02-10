/// <summary>
/// The mod settings that can be adjusted by a user, usually from the ModSelector.
/// </summary>
public class ModSettingsJSON
{
    public bool RDRTS { get; set; }

    public static bool Get()
    {
        return new ModConfig<ModSettingsJSON>("DanielDice-modsettings").Read().RDRTS;
    }
}