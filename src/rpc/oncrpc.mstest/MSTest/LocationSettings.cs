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
        // Get the type of the class that declares this method.
        Type declaringType = System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!;

        // Get the AssemblyFileInfo for the assembly that contains the declaring type and
        // append '.Settings' to the assembly name to build the JSon settings file name.

        AssemblyFileInfo ai = new( declaringType.Assembly, null, ".Settings", ".json" );

        // copy application context settings if files do not exist or to clear corrupted settings.

        AppSettingsScribe.InitializeSettingsFiles( ai, System.Diagnostics.Debugger.IsAttached, System.Diagnostics.Debugger.IsAttached );

        // read the settings using the default serializer and document options.

        LocationSettings ti = new()
        {
            FilePath = ai.AllUsersAssemblyFilePath ?? ai.ThisUserAssemblyFilePath ?? ai.AppContextAssemblyFilePath ?? string.Empty,
            SectionName = declaringType.Name
        };

        ti.Scribe = new( [ti.SectionName], [ti], ai );

        ti.FilePath = ti.Scribe.UserSettingsPath;

        ti.ReadSettings();

        if ( !ti.Scribe.SettingsExist( ti.FilePath, out string details ) )
            throw new InvalidOperationException( $"Failed reading the settings; {details}" );

        return ti;
    }

    /// <summary>   Gets the instance. </summary>
    /// <value> The instance. </value>
    public static LocationSettings Instance => _instance.Value;

    private static readonly Lazy<LocationSettings> _instance = new( LocationSettings.CreateInstance, true );

    #endregion
}
