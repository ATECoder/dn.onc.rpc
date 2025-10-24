using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace cc.isr.Std.MSTest;

/// <summary>   Provides settings for all tests. </summary>
/// <remarks>   2023-04-24. </remarks>
internal sealed class LocationSettings : cc.isr.Json.AppSettings.Settings.LocationSettingsBase
{
    #region " singleton "

    /// <summary>   Creates an instance of the <see cref="LocationSettings"/> after restoring the
    /// application context settings to both the user and all user files. </summary>
    /// <remarks>   2023-05-15. </remarks>
    /// <returns>   The new instance. </returns>
    private static LocationSettings CreateInstance()
    {
        // Get the method declaring type for the assembly file information and the settings section name.
        Type declaringType = System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!;

        // get assembly files using the .Settings suffix.

        AssemblyFileInfo ai = new( declaringType.Assembly, null, ".Settings", ".json" );

        // must copy application context settings here to clear any bad settings files.

        AppSettingsScribe.InitializeSettingsFiles( ai, true, true );

        // read the settings using the default serializer and document options.

        LocationSettings ti = new()
        {
            FilePath = ai.AllUsersAssemblyFilePath ?? ai.ThisUserAssemblyFilePath ?? ai.AppContextAssemblyFilePath ?? string.Empty,
            SectionName = declaringType.Name
        };
        ti.ReadSettings();

        return ti;
    }

    /// <summary>   Gets the instance. </summary>
    /// <value> The instance. </value>
    public static LocationSettings Instance => _instance.Value;

    private static readonly Lazy<LocationSettings> _instance = new( LocationSettings.CreateInstance, true );

    #endregion

    #region " i/o "

    /// <summary>   Reads the settings. </summary>
    /// <remarks>   2023-05-23. </remarks>
    public override void ReadSettings()
    {
        base.ReadSettings( this );
    }

    /// <summary>   Saves the settings. </summary>
    /// <remarks>   2023-05-23. </remarks>
    public override void SaveSettings()
    {
        base.SaveSettings( this );
    }

    #endregion
}
