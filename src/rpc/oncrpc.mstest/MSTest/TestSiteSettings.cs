using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace cc.isr.Std.MSTest;

/// <summary>   Provides settings for all tests. </summary>
/// <remarks>   2023-04-24. </remarks>
internal sealed class TestSiteSettings : cc.isr.Std.Tests.TestSiteSettings
{
    #region " construction "

    /// <summary>   Default constructor. </summary>
    /// <remarks>   2023-05-09. </remarks>
    public TestSiteSettings()
    { }

    #endregion

    #region " singleton "

    /// <summary>   Creates an instance of the <see cref="TestSiteSettings"/> after restoring the
    /// application context settings to both the user and all user files. </summary>
    /// <remarks>   2023-05-15. </remarks>
    /// <returns>   The new instance. </returns>
    private static TestSiteSettings CreateInstance()
    {
        // Get the method declaring type for the assembly file information and the settings section name.
        Type declaringType = System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!;

        // get assembly files using the .Settings suffix.

        AssemblyFileInfo ai = new( declaringType.Assembly, null, ".Settings", ".json" );

        // must copy application context settings here to clear any bad settings files.

        // must copy application context settings here to clear any bad settings files.

        AppSettingsScribe.InitializeSettingsFiles( ai, true, true );

        // set the settings path and section name for reading and writing the settings as necessary.

        cc.isr.Std.Tests.TestSiteSettings.SettingsPath = ai.AllUsersAssemblyFilePath!;
        cc.isr.Std.Tests.TestSiteSettings.SettingsSectionName = declaringType.Name;

        // read the settings using the default serializer and document options.

        TestSiteSettings ti = new();
        ti.ReadSettings();

        return ti;
    }

    /// <summary>   Gets the instance. </summary>
    /// <value> The instance. </value>
    public static new TestSiteSettings Instance => _instance.Value;

    private static readonly Lazy<TestSiteSettings> _instance = new( CreateInstance, true );

    #endregion
}
