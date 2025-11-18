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

    /// <summary>   Creates the scribe. </summary>
    /// <remarks>   2025-10-30. </remarks>
    public override void CreateScribe()
    {
        this.Scribe = string.IsNullOrWhiteSpace( this.SectionName )
            ? new( [this] )
            : new( [this.SectionName], [this] );
    }

    /// <summary>   Creates an instance of the <see cref="LocationSettings"/> after restoring the
    /// application context settings to both the user and all user files. </summary>
    /// <remarks>   2023-05-15. </remarks>
    /// <returns>   The new instance. </returns>
    private static LocationSettings CreateInstance()
    {
        // Get the type of the class that declares this method.
        Type declaringType = System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!;

        LocationSettings ti = new()
        {
            SectionName = declaringType.Name
        };

        bool overwrite = Json.AppSettings.Models.AppSettingsScribe.IsDebuggingOrTesting();

        ti.ReadSettings( declaringType, ".Settings", overwrite, overwrite );

        return ti;
    }

    /// <summary>   Gets the instance. </summary>
    /// <value> The instance. </value>
    public static LocationSettings Instance => _instance.Value;

    private static readonly Lazy<LocationSettings> _instance = new( LocationSettings.CreateInstance, true );

    #endregion
}
