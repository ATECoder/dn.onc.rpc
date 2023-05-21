using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.EnumExtensions;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest;

/// <summary>   (Unit Test Class) a support tests. </summary>
[TestClass]
public class SupportTests
{

    #region " fixture Construction and Cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="testContext"> Gets or sets the test context which provides information about
    /// and functionality for the current test run. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext testContext )
    {
        try
        {
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
        }
        catch ( Exception ex )
        {
            Logger.Writer.LogMemberError( $"Failed initializing fixture:", ex );
            CleanupFixture();
        }
    }

    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the
    /// current test run.
    /// </summary>
    /// <value> The test context. </value>
    public TestContext? TestContext { get; set; }

    private static TestContext? _classTestContext;

    /// <summary>   Cleanup fixture. </summary>
    [ClassCleanup]
    public static void CleanupFixture()
    { }
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
