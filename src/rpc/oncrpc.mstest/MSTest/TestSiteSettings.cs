namespace cc.isr.MSTest;

/// <summary>   Provides settings for all tests. </summary>
/// <remarks>   2023-04-24. </remarks>
internal sealed class TestSiteSettings : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
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
        // get assembly files using the .Logging suffix.

        AssemblyFileInfo ai = new( typeof( TestSiteSettings ).Assembly, null, ".Settings", ".json" );

        // must copy application context settings here to clear any bad settings files.

        AppSettingsScribe.CopySettings( ai.AppContextAssemblyFilePath!, ai.AllUsersAssemblyFilePath! );
        AppSettingsScribe.CopySettings( ai.AppContextAssemblyFilePath!, ai.ThisUserAssemblyFilePath! );

        TestSiteSettings ti = new();
        AppSettingsScribe.ReadSettings( ai.AllUsersAssemblyFilePath!, nameof( TestSiteSettings ), ti );

        return ti;
    }

    /// <summary>   Gets the instance. </summary>
    /// <value> The instance. </value>
    public static TestSiteSettings Instance => _instance.Value;

    private static readonly Lazy<TestSiteSettings> _instance = new( CreateInstance, true );

    #endregion

    #region " test site configuration "

    private TraceLevel _messageLevel = TraceLevel.Off;
    /// <summary>   Gets or sets the trace level. </summary>
    /// <remarks>
    /// This property name is different from the <see cref="Text.Json"/> property name in
    /// order to ensure that the class is correctly serialized. It's value is initialized as <see cref="TraceLevel.Off"/>
    /// in order to test the reading from the settings file.
    /// </remarks>
    /// <value> The message <see cref="TraceLevel"/>. </value>
    [System.ComponentModel.Description( "Sets the message level" )]
    [JsonPropertyName( "TraceLevel" )]
    public TraceLevel MessageLevel
    {
        get => this._messageLevel;
        set => _ = this.SetProperty( ref this._messageLevel, value );
    }

    private bool _enabled = true;
    /// <summary>   Gets or sets a value indicating whether this object is enabled. </summary>
    /// <value> True if enabled, false if not. </value>
    [System.ComponentModel.Description( "True if testing is enabled for this test class" )]
    public bool Enabled
    {
        get => this._enabled;
        set => this.SetProperty( ref this._enabled, value );
    }

    private bool _all = true;
    /// <summary> Gets or sets all. </summary>
    /// <value> all. </value>
    [System.ComponentModel.Description( "True if all testing is enabled for this test class" )]
    public bool All
    {
        get => this._all;
        set => this.SetProperty( ref this._all, value );
    }

    #endregion

    #region " test site location information "

    private string _iPv4Prefixes = "192.168|10.1";
    /// <summary>   Gets or sets the candidate IPv4 prefixes for this location. </summary>
    /// <value> The IPv4 prefixes. </value>
    [System.ComponentModel.Description( "Specifies the IPv4 prefixes of the Internet addresses of known test sites; used to select settings that are test-site specific" )]
    public string IPv4Prefixes
    {
        get => this._iPv4Prefixes;
        set => this.SetProperty( ref this._iPv4Prefixes, value );
    }

    private string _timeZones = "Pacific Standard Time|Central Standard Time";
    /// <summary> Gets or sets the candidate time zones of this location. </summary>
    /// <value> The candidate time zones of the test site. </value>
    [System.ComponentModel.Description( "Specifies the time zone identities of known test sites corresponding to the set of IPv4 prefixes" )]
    public string TimeZones
    {
        get => this._timeZones;
        set => this.SetProperty( ref this._timeZones, value );
    }

    private string _timeZoneOffsets = "-8|-6";
    /// <summary> Gets or sets the candidate time zone offsets of this location. </summary>
    /// <value> The time zone offsets. </value>
    [System.ComponentModel.Description( "Specifies the time zone offsets of known test sites corresponding to the set of IPv4 prefixes" )]
    public string TimeZoneOffsets
    {
        get => this._timeZoneOffsets;
        set => this.SetProperty( ref this._timeZoneOffsets, value );
    }

    #endregion

    #region " test site location identification "

    private string _timeZone = "Pacific Standard Time";
    /// <summary> Gets the time zone of the test site. </summary>
    /// <value> The time zone of the test site. </value>
    [System.ComponentModel.Description( "Gets the time zone identity of the test site" )]
    public string TimeZone()
    {
        if ( string.IsNullOrWhiteSpace( this._timeZone ) )
            this._timeZone = this.TimeZones.Split( '|' )[this.HostInfoIndex()];
        return this._timeZone;
    }

    private double _timeZoneOffset = double.MinValue;
    /// <summary> Gets the time zone offset of the test site. </summary>
    /// <value> The time zone offset of the test site. </value>
    [System.ComponentModel.Description( "Gets the time zone offset of the test site" )]
    public double TimeZoneOffset()
    {
        if ( this._timeZoneOffset == double.MinValue )
            this._timeZoneOffset = double.Parse( this.TimeZoneOffsets.Split( '|' )[this.HostInfoIndex()], System.Globalization.CultureInfo.InvariantCulture );
        return this._timeZoneOffset;
    }

    /// <summary> Gets the host name of the test site. </summary>
    /// <value> The host name of the test site. </value>
    [System.ComponentModel.Description( "Gets the host name of the test site" )]
    public static string HostName()
    {
        return System.Net.Dns.GetHostName();
    }

    private System.Net.IPAddress _hostAddress = System.Net.IPAddress.None;
    /// <summary> Gets the IP address of the test site. </summary>
    /// <value> The IP address of the test site. </value>
    [System.ComponentModel.Description( "Gets the host address of the test site" )]
    public System.Net.IPAddress? HostAddress()
    {
        if ( this._hostAddress is null || this._hostAddress == System.Net.IPAddress.None )
            foreach ( System.Net.IPAddress value in System.Net.Dns.GetHostEntry( HostName() ).AddressList )
                if ( value.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
                {
                    this._hostAddress = value;
                    break;
                }
        return this._hostAddress;
    }

    private int _hostInfoIndex = -1;
    /// <summary> Gets the index into the host information strings. </summary>
    /// <value> The index into the host information strings. </value>
    [System.ComponentModel.Description( "Gets the host info index of the test site that corresponds to the IPv4 prefix that matches the host address" )]
    public int HostInfoIndex()
    {
        if ( this._hostInfoIndex < 0 )
        {
            string ip = this.HostAddress()?.ToString() ?? string.Empty;
            int i = -1;
            foreach ( string value in this.IPv4Prefixes.Split( '|' ) )
            {
                i += 1;
                if ( ip.StartsWith( value, StringComparison.OrdinalIgnoreCase ) )
                {
                    this._hostInfoIndex = i;
                    break;
                }
            }
        }

        return this._hostInfoIndex;
    }

    #endregion

    #region " location equality implementation "

    public override bool Equals( object? obj )
    {
        return obj is TestSiteSettings settings && this._timeZones == settings._timeZones;
    }

    public override int GetHashCode()
    {
        return this.TimeZones.GetHashCode();
    }

    #endregion

}
