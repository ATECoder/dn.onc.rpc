namespace cc.isr.ONC.RPC.Web;

/// <summary>
/// The <see cref="Base64"/> class provides static methods to convert back and forth
/// between binary and base64-encoded data.
/// </summary>
/// <remarks> <para>
///  
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public static class Base64
{
    /// <summary>   Converts binary data into base64 encoded data. </summary>
    /// <param name="binaryData">       Binary data to be encoded. </param>
    /// <param name="binaryOffset">     Offset into <paramref name="binaryData"/> where to the data to be
    ///                                 encoded begins. </param>
    /// <param name="length">           Length of data to encode. </param>
    /// <param name="encodedData">      Buffer receiving base64 encoded data. </param>
    /// <param name="encodedOffset">    Offset into <paramref name="encodedData"/> where the store base64
    ///                                 encoded data. </param>
    /// <returns>   Length of encoded base64 data. </returns>
    public static int Encode( byte[] binaryData, int binaryOffset, int length, byte[] encodedData, int encodedOffset )
    {
        // Calculate length of encoded data including optional padding.
        int encodedLength = (length + 2) / 3 * 4;

        // Now do the encoding, thus inflating every three bytes of binary
        // data to four ASCII characters.
        int b1;
        int b2;
        int b3;
        int endPos = binaryOffset + length - 1 - 2;
        while ( binaryOffset <= endPos )
        {
            b1 = binaryData[binaryOffset++];
            b2 = binaryData[binaryOffset++];
            b3 = binaryData[binaryOffset++];
            encodedData[encodedOffset++] = _encodingBase64Alephbeth[b1 >> (2 & 0x1f) & unchecked(0x3F)];
            encodedData[encodedOffset++] = _encodingBase64Alephbeth[b1 << 4 & unchecked(0x30) | b2 >> (4 & 0x1f) & unchecked(0xF)];
            encodedData[encodedOffset++] = _encodingBase64Alephbeth[b2 << 2 & unchecked(0x3C) | b3 >> (6 & 0x1f) & unchecked(0x03)];
            encodedData[encodedOffset++] = _encodingBase64Alephbeth[b3 & unchecked(0x3F)];
        }
        // If one or two bytes are left (because we work on blocks of three
        // bytes), convert them too and apply padding.
        endPos += 2;

        // now points to the last encodable byte
        if ( binaryOffset <= endPos )
        {
            b1 = binaryData[binaryOffset++];
            encodedData[encodedOffset++] = _encodingBase64Alephbeth[b1 >> (2 & 0x1f) & unchecked(0x3F)];
            if ( binaryOffset <= endPos )
            {
                b2 = binaryData[binaryOffset++];
                encodedData[encodedOffset++] = _encodingBase64Alephbeth[b1 << 4 & unchecked(0x30) | b2 >> (4 & 0x1f) & unchecked(0xF)];
                encodedData[encodedOffset++] = _encodingBase64Alephbeth[b2 << 2 & unchecked(0x3C)];
                encodedData[encodedOffset] = ( byte ) '=';
            }
            else
            {
                encodedData[encodedOffset++] = _encodingBase64Alephbeth[b1 << 4 & unchecked(0x30)];
                encodedData[encodedOffset++] = ( byte ) '=';
                encodedData[encodedOffset] = ( byte ) '=';
            }
        }
        // Finally return length of encoded data
        return encodedLength;
    }

    /// <summary>   Converts base64 encoded data into binary data. </summary>
    /// <param name="encodedData">      Base64 encoded data. </param>
    /// <param name="encodedOffset">    Offset into <paramref name="encodedData"/> where the base64 encoded
    ///                                 data starts. </param>
    /// <param name="length">           Length of encoded data. </param>
    /// <param name="binaryData">       Decoded (binary) data. </param>
    /// <param name="binaryOffset">     Offset into <paramref name="binaryData"/> where to store the
    ///                                 decoded binary data. </param>
    /// <returns>   Length of decoded binary data. </returns>
    public static int Decode( byte[] encodedData, int encodedOffset, int length, byte[] binaryData, int binaryOffset )
    {
        // Determine the length of data to be decoded. Optional padding has
        // to be removed first (of course).
        int endPos = encodedOffset + length - 1;
        while ( endPos >= 0 && encodedData[endPos] == '=' )
            --endPos;
        // next line was: endPos - length / 4 + 1
        int binaryLength = endPos - encodedOffset - length / 4 + 1;

        // Now do the four-to-three entities/letters/bytes/whatever
        // conversion. We chew on as many four-letter groups as we can,
        // converting them into three byte groups.
        byte b1;
        byte b2;
        byte b3;
        byte b4;
        int stopPos = endPos - 3;
        // now points to the last letter in the
        // last four-letter group
        while ( encodedOffset <= stopPos )
        {
            b1 = _decodingBase64Alephbeth[encodedData[encodedOffset++]];
            b2 = _decodingBase64Alephbeth[encodedData[encodedOffset++]];
            b3 = _decodingBase64Alephbeth[encodedData[encodedOffset++]];
            b4 = _decodingBase64Alephbeth[encodedData[encodedOffset++]];
            binaryData[binaryOffset++] = ( byte ) (b1 << 2 & unchecked(0xFF) | b2 >> (4 & 0x1f) & unchecked(0x03));
            binaryData[binaryOffset++] = ( byte ) (b2 << 4 & unchecked(0xFF) | b3 >> (2 & 0x1f) & unchecked(0x0F));
            binaryData[binaryOffset++] = ( byte ) (b3 << 6 & unchecked(0xFF) | b4 & unchecked(0x3F));
        }
        // If one, two or three letters from the base64 encoded data are
        // left, convert them too.
        // Hack Note: if the length of encoded data is not a multiple
        // of four, then padding must occur ('='). As the decoding alphabet
        // contains zeros everywhere with the exception of valid letters,
        // indexing into the mapping is just fine and reliefs us of the
        // pain to check everything and make thus makes the code better.
        if ( encodedOffset <= endPos )
        {
            b1 = _decodingBase64Alephbeth[encodedData[encodedOffset++]];
            b2 = _decodingBase64Alephbeth[encodedData[encodedOffset++]];
            binaryData[binaryOffset++] = ( byte ) (b1 << 2 & unchecked(0xFF) | b2 >> (4 & 0x1f) & unchecked(0x03));
            if ( encodedOffset <= endPos )
            {
                b3 = _decodingBase64Alephbeth[encodedData[encodedOffset]];
                binaryData[binaryOffset++] = ( byte ) (b2 << 4 & unchecked(0xFF) | b3 >> (2 & 0x1f) & unchecked(0x0F));
            }
        }
        // Okay. That's it for now. Just return the length of decoded data.
        return binaryLength;
    }

    /// <summary>Mapping from binary 0-63 to base64 alphabet according to RFC 2045.</summary>
    private static readonly byte[] _encodingBase64Alephbeth = new byte[] { (byte) 'A',(byte) 'B',(byte) 'C',(byte) 'D',(byte) 'E',
        (byte) 'F',(byte) 'G',(byte) 'H',(byte) 'I',(byte) 'J',(byte) 'K', (byte) 'L',(byte) 'M',(byte) 'N',(byte) 'O',(byte) 'P',
        (byte) 'Q',(byte) 'R',(byte) 'S',(byte) 'T',(byte) 'U',(byte) 'V',(byte) 'W',(byte) 'X',(byte) 'Y',(byte) 'Z',
        (byte) 'a',(byte) 'b',(byte) 'c',(byte) 'd',(byte) 'e',(byte) 'f',(byte) 'g',(byte) 'h',(byte) 'i',(byte) 'j',
        (byte) 'k',(byte) 'l',(byte) 'm',(byte) 'n',(byte) 'o',(byte) 'p',(byte) 'q',(byte) 'r',(byte) 's',(byte) 't',
        (byte) 'u',(byte) 'v',(byte) 'w',(byte) 'x',(byte) 'y',(byte) 'z',
        (byte) '0',(byte) '1',(byte) '2',(byte) '3',(byte) '4',(byte) '5',(byte) '6',(byte) '7',(byte) '8',(byte) '9',(byte) '+',(byte) '/' };

    /// <summary>Mapping from base64 alphabet to binary 0-63.</summary>
    private static readonly byte[] _decodingBase64Alephbeth;

    /// <summary>   Static constructor. </summary>
    /// <remarks>   2023-01-02. </remarks>
    static Base64()
    {
        _decodingBase64Alephbeth = new byte[256];
        for ( int i = 0; i < 64; ++i )
            _decodingBase64Alephbeth[_encodingBase64Alephbeth[i]] = ( byte ) i;
    }
}
