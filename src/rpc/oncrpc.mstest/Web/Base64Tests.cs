using System.Text;

using cc.isr.ONC.RPC.Web;

namespace cc.isr.ONC.RPC.MSTest.Web;

/// <summary>   (Unit Test Class) a base 64 tests. </summary>
/// <remarks>   2022-12-20. </remarks>
[TestClass]
public class Base64Tests
{
    /// <summary>   (Unit Test Method) base 64 should encode and decode. </summary>
    /// <remarks>   2022-12-20. </remarks>
    [TestMethod]
    public void Base64ShouldEncodeAndDecode()
    {
        byte[] source = Encoding.ASCII.GetBytes( "The Foxboro jumps over the lazy I/A" );
        this.AssertBase64ShouldEncodeAndDecode( "test-1", source, 1, 4 );
        this.AssertBase64ShouldEncodeAndDecode( "test-2", source, 2, 4 );
        this.AssertBase64ShouldEncodeAndDecode( "test-3", source, 3, 4 );
        this.AssertBase64ShouldEncodeAndDecode( "test-4", source, 4, 8 );
        this.AssertBase64ShouldEncodeAndDecode( "test-5", source, source.Length, (source.Length + 2) / 3 * 4 );
    }

    /// <summary>   Assert base 64 should encode and decode. </summary>
    /// <remarks>   2022-12-20. </remarks>
    /// <param name="test">         The test. </param>
    /// <param name="source">       Source for the. </param>
    /// <param name="lenSource">    The length source. </param>
    /// <param name="lenEncoded">   The length encoded. </param>
    public virtual void AssertBase64ShouldEncodeAndDecode( string test, byte[] source, int lenSource, int lenEncoded )
    {
        byte[] encoded = new byte[((source.Length + 2) / 3 * 4)];
        byte[] decoded = new byte[source.Length];
        Console.Out.Write( test + ": " );
        int len = Base64.Encode( source, 0, lenSource, encoded, 0 );
        Assert.AreEqual( len, lenEncoded, $"Test {test} failed. Expected encoded length = {lenEncoded}, got length =  {len}" );

        len = Base64.Decode( encoded, 0, len, decoded, 0 );
        Assert.AreEqual( len, lenSource, $"Test {test} failed. Decoded length mismatch, expected {lenSource}, got {len}" );
    }

}
