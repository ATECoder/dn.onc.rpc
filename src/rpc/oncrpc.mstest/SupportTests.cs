using System.Diagnostics;

using cc.isr.ONC.RPC.EnumExtensions;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest;

/// <summary>   (Unit Test Class) a support tests. </summary>
[TestClass]
public class SupportTests
{

    #region " fixture construction and cleanup "

    /// <summary> Initializes the test class before running the first test. </summary>
    /// <param name="testContext"> Gets or sets the test context which provides information about
    /// and functionality for the current test run. </param>
    /// <remarks>Use ClassInitialize to run code before running the first test in the class</remarks>
    [ClassInitialize()]
    public static void InitializeTestClass( TestContext testContext )
    {
        try
        {
            string methodFullName = $"{testContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}";
            if ( Logger is null )
                Console.WriteLine( methodFullName );
            else
                Logger?.LogMemberInfo( methodFullName );
        }
        catch ( Exception ex )
        {
            if ( Logger is null )
                Console.WriteLine( $"Failed initializing the test class: {ex}" );
            else
                Logger.LogMemberError( "Failed initializing the test class:", ex );

            // cleanup to meet strong guarantees

            try
            {
                CleanupTestClass();
            }
            finally
            {
            }
        }
    }

    /// <summary> Cleans up the test class after all tests in the class have run. </summary>
    /// <remarks> Use <see cref="CleanupTestClass"/> to run code after all tests in the class have run. </remarks>
    [ClassCleanup()]
    public static void CleanupTestClass()
    { }

    private IDisposable? _loggerScope;

    private LoggerTraceListener<SupportTests>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<SupportTests>( Logger );
            _ = Trace.Listeners.Add( this._traceListener );
        }
    }

    /// <summary> Cleans up the test class instance after each test has run. </summary>
    [TestCleanup()]
    public void CleanupAfterEachTest()
    {
        Assert.IsFalse( this._traceListener?.Any( TraceEventType.Error ),
            $"{nameof( this._traceListener )} should have no {TraceEventType.Error} messages" );
        this._loggerScope?.Dispose();
        this._traceListener?.Dispose();
        Trace.Listeners.Clear();
    }

    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the
    /// current test run.
    /// </summary>
    /// <value> The test context. </value>
    public TestContext? TestContext { get; set; }

    /// <summary>   Gets a logger instance for this category. </summary>
    /// <value> The logger. </value>
    public static ILogger<SupportTests>? Logger { get; } = LoggerProvider.InitLogger<SupportTests>();

    #endregion

    #region " initialization tests "

    /// <summary>   (Unit Test Method) 00 logger should be enabled. </summary>
    /// <remarks>   2023-05-31. </remarks>
    [TestMethod]
    public void A00LoggerShouldBeEnabled()
    {
        Assert.IsNotNull( Logger, $"{nameof( Logger )} should initialize" );
        Assert.IsTrue( Logger.IsEnabled( LogLevel.Information ),
            $"{nameof( Logger )} should be enabled for the {LogLevel.Information} {nameof( LogLevel )}" );
    }

    /// <summary>   (Unit Test Method) 01 logger trace listener should have messages. </summary>
    /// <remarks>   2023-06-01. </remarks>
    [TestMethod]
    public void A01LoggerTraceListenerShouldHaveMessages()
    {
        Assert.IsNotNull( this._traceListener, $"{nameof( this._traceListener )} should initialize" );
        Assert.IsTrue( Trace.Listeners.Count > 0, $"{nameof( Trace )} should have non-zero {nameof( Trace.Listeners )}" );
        Trace.TraceInformation( "Testing tracing an info message" ); Trace.Flush();
        Assert.IsTrue( this._traceListener?.Any( TraceEventType.Information ), $"{nameof( this._traceListener )} should have {TraceEventType.Error} messages" );

        // no need to report errors for this test.

        this._traceListener?.Clear();
    }

    #endregion

    #region " support "

    /// <summary>   Assert should get description. </summary>
    /// <param name="value">                The value. </param>
    /// <param name="expectedDescription">  Information describing the expected. </param>
    private static void AssertShouldGetDescription( OncRpcMessageType value, string expectedDescription )
    {
        string actual = value.GetDescription();
        Assert.AreEqual( expectedDescription, actual );
    }

    /// <summary>   (Unit Test Method) message type should get description. </summary>
    [TestMethod]
    public void MessageTypeShouldGetDescription()
    {
        AssertShouldGetDescription( OncRpcMessageType.NotSpecified, "Unspecified ONC/RPC message type." );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="OncRpcAuthType"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToAuthType( int expected )
    {
        OncRpcAuthType actual = expected.ToAuthType();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="OncRpcAuthType"/>. </summary>
    [TestMethod]
    public void IntShouldCastToAuthType()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( OncRpcAuthType ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToAuthType( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToAuthType( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="OncRpcAcceptStatus"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToAcceptStatus( int expected )
    {
        OncRpcAcceptStatus actual = expected.ToAcceptStatus();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="OncRpcAcceptStatus"/>. </summary>
    [TestMethod]
    public void IntShouldCastToAcceptStatus()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( OncRpcAcceptStatus ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToAcceptStatus( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToAcceptStatus( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="OncRpcAuthStatus"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToAuthStatus( int expected )
    {
        OncRpcAuthStatus actual = expected.ToAuthStatus();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="OncRpcAuthStatus"/>. </summary>
    [TestMethod]
    public void IntShouldCastToAuthStatus()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( OncRpcAuthStatus ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToAuthStatus( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToAuthStatus( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="OncRpcMessageType"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToMessageType( int expected )
    {
        OncRpcMessageType actual = expected.ToMessageType();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="OncRpcMessageType"/>. </summary>
    [TestMethod]
    public void IntShouldCastToMessageType()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( OncRpcMessageType ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToMessageType( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToMessageType( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="OncRpcExceptionReason"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToExceptionReason( int expected )
    {
        OncRpcExceptionReason actual = expected.ToExceptionReason();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="OncRpcExceptionReason"/>. </summary>
    [TestMethod]
    public void IntShouldCastToExceptionReason()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( OncRpcExceptionReason ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToExceptionReason( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToExceptionReason( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="OncRpcProtocol"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToProtocols( int expected )
    {
        OncRpcProtocol actual = expected.ToProtocols();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="OncRpcProtocol"/>. </summary>
    [TestMethod]
    public void IntShouldCastToProtocols()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( OncRpcProtocol ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToProtocols( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToProtocols( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="OncRpcRejectStatus"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToRejectStatus( int expected )
    {
        OncRpcRejectStatus actual = expected.ToRejectStatus();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="OncRpcRejectStatus"/>. </summary>
    [TestMethod]
    public void IntShouldCastToRejectStatus()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( OncRpcRejectStatus ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToRejectStatus( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToRejectStatus( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="OncRpcReplyStatus"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToReplyStatus( int expected )
    {
        OncRpcReplyStatus actual = expected.ToReplyStatus();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="OncRpcReplyStatus"/>. </summary>
    [TestMethod]
    public void IntShouldCastToReplyStatus()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( OncRpcReplyStatus ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToReplyStatus( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToReplyStatus( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="OncRpcRetransmitMode"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToRetransmitMode( int expected )
    {
        OncRpcRetransmitMode actual = expected.ToRetransmitMode();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="OncRpcRetransmitMode"/>. </summary>
    [TestMethod]
    public void IntShouldCastToRetransmitMode()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( OncRpcRetransmitMode ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToRetransmitMode( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToRetransmitMode( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="OncRpcPortmapServiceProcedure"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToPortmapServiceProcedure( int expected )
    {
        OncRpcPortmapServiceProcedure actual = expected.ToPortmapServiceProcedure();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="OncRpcPortmapServiceProcedure"/>. </summary>
    [TestMethod]
    public void IntShouldCastToPortmapServiceProcedure()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( OncRpcPortmapServiceProcedure ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToPortmapServiceProcedure( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToPortmapServiceProcedure( maxValue + 1 ); } );
    }

    #endregion

}
