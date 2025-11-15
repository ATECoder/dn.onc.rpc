namespace cc.isr.Std.Tests.Extensions;

/// <summary>   A <see cref="System.Random"/> extensions. </summary>
/// <remarks>   2024-07-26. </remarks>
public static partial class RandomExtensions
{
    /// <summary> Generates normally distributed doubles. </summary>
    /// <remarks> David, 2020-09-23. </remarks>
    /// <param name="generator"> The random number generator. </param>
    /// <param name="count">     Number of values to generate. </param>
    /// <returns>
    /// An enumerator that allows for each to be used to process the random normal doubles in this
    /// collection.
    /// </returns>
    public static IList<double> GenerateRandomNormals( this Random generator, int count )
    {
        List<double> l = [];
        for ( int i = 1, loopTo = count; i <= loopTo; i++ )
        {
            l.Add( NextNormal( generator ) );
        }

        return [.. l];
    }

    /// <summary>
    /// Generates the next random normally-distributed mean zero unity standard deviation number.
    /// </summary>
    /// <remarks>   Uses the Box-Mueller method. </remarks>
    /// <exception cref="ArgumentNullException">    Thrown when one or more required arguments are
    ///                                             null. </exception>
    /// <param name="random">   The pseudo random number generator. </param>
    /// <returns>   A Double. </returns>
    public static double NextNormal( this Random random )
    {
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull( random, nameof( random ) );
#else
        if ( random is null ) throw new ArgumentNullException( nameof( random ) );
#endif

        double x, y, r2;
        do
        {
            x = (2.0d * random.NextDouble()) - 1.0d;
            y = (2.0d * random.NextDouble()) - 1.0d;
            r2 = (x * x) + (y * y);
        }
        while ( r2 is >= 1.0d or 0.0d );
        return x * Math.Sqrt( -2.0d * Math.Log( r2 ) / r2 );
    }

}
