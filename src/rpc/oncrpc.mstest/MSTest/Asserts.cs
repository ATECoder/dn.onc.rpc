using System;
using System.Collections.Generic;
using System.Linq;

namespace cc.isr.Std.MSTest;

/// <summary> Assert. </summary>
/// <remarks> David, 2016-12-12 <para>
/// <see href="https://StackOverflow.com/questions/933613/how-do-i-use-assert-to-verify-that-an-exception-has-been-thrown" />
/// </para>
/// </remarks>
public sealed class Asserts
{
    #region " singleton "

    /// <summary>
    /// Constructor that prevents a default instance of this class from being created.
    /// </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    private Asserts() : base() { }

    private static readonly Lazy<Asserts> _lazyAsserts = new( () => new(), true );

    /// <summary>   Gets the instance of the <see cref="Asserts"/> class. </summary>
    /// <remarks>
    /// Use this property to instantiate a single instance of this class. This class uses lazy
    /// instantiation, meaning the instance isn't created until the first time it's retrieved.
    /// </remarks>
    /// <value> A new or existing instance of the <see cref="Asserts"/> class. </value>
    public static Asserts Instance => _lazyAsserts.Value;

    /// <summary> Returns true if an instance of the class was created and not disposed. </summary>
    /// <value> <c>true</c> if instantiated; otherwise, <c>false</c>. </value>
    public static bool Instantiated => _lazyAsserts.IsValueCreated;

    #endregion

    #region " time span "

    /// <summary> Are equal. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <exception cref="AssertFailedException"> Thrown when an Assert Failed error condition occurs. </exception>
    /// <param name="expected"> The expected. </param>
    /// <param name="actual">   The actual. </param>
    /// <param name="delta">    The delta. </param>
    /// <param name="message">  The message. </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1822:Mark members as static", Justification = "<Pending>" )]
    public void AreEqual( TimeSpan expected, TimeSpan actual, TimeSpan delta, string message )
    {
        if ( actual < expected.Subtract( delta ) || actual > expected.Add( delta ) )
        {
            throw new AssertFailedException( $"Expected {expected} <> actual {actual} by more than {delta}; {message}" );
        }
    }

    /// <summary> Are equal. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <param name="expected"> The expected. </param>
    /// <param name="actual">   The actual. </param>
    /// <param name="delta">    The delta. </param>
    /// <param name="format">   Describes the format to use. </param>
    /// <param name="args">     A variable-length parameters list containing arguments. </param>
    public void AreEqual( TimeSpan expected, TimeSpan actual, TimeSpan delta, string format, params object[] args )
    {
        this.AreEqual( expected, actual, delta, string.Format( System.Globalization.CultureInfo.CurrentCulture, format, args ) );
    }

    #endregion

    #region " date time "

    /// <summary> Are equal. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <exception cref="AssertFailedException"> Thrown when an Assert Failed error condition occurs. </exception>
    /// <param name="expected"> The expected. </param>
    /// <param name="actual">   The actual. </param>
    /// <param name="delta">    The delta. </param>
    /// <param name="message">  The message. </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1822:Mark members as static", Justification = "<Pending>" )]
    public void AreEqual( DateTime expected, DateTime actual, TimeSpan delta, string message )
    {
        if ( actual < expected.Subtract( delta ) || actual > expected.Add( delta ) )
        {
            throw new AssertFailedException( $"Expected {expected} > actual {actual} by {expected.Subtract( actual ).Subtract( delta )} over {delta}; {message}" );
        }
    }

    /// <summary> Are equal. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <param name="expected"> The expected. </param>
    /// <param name="actual">   The actual. </param>
    /// <param name="delta">    The delta. </param>
    /// <param name="format">   Describes the format to use. </param>
    /// <param name="args">     A variable-length parameters list containing arguments. </param>
    public void AreEqual( DateTime expected, DateTime actual, TimeSpan delta, string format, params object[] args )
    {
        this.AreEqual( expected, actual, delta, string.Format( System.Globalization.CultureInfo.CurrentCulture, format, args ) );
    }

    #endregion

    #region " date time offset "

    /// <summary> Are equal. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <exception cref="AssertFailedException"> Thrown when an Assert Failed error condition occurs. </exception>
    /// <param name="expected"> The expected. </param>
    /// <param name="actual">   The actual. </param>
    /// <param name="delta">    The delta. </param>
    /// <param name="message">  The message. </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1822:Mark members as static", Justification = "<Pending>" )]
    public void AreEqual( DateTimeOffset expected, DateTimeOffset actual, TimeSpan delta, string message )
    {
        if ( actual < expected.Subtract( delta ) || actual > expected.Add( delta ) )
        {
            throw new AssertFailedException( $"Expected {expected:o} <> actual {actual:o} by more than {delta}; {message}" );
        }
    }

    /// <summary> Are equal. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <param name="expected"> The expected. </param>
    /// <param name="actual">   The actual. </param>
    /// <param name="delta">    The delta. </param>
    /// <param name="format">   Describes the format to use. </param>
    /// <param name="args">     A variable-length parameters list containing arguments. </param>
    public void AreEqual( DateTimeOffset expected, DateTimeOffset actual, TimeSpan delta, string format, params object[] args )
    {
        this.AreEqual( expected, actual, delta, string.Format( System.Globalization.CultureInfo.CurrentCulture, format, args ) );
    }

    #endregion

    #region " is single "

    /// <summary> Is single. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <param name="items"> The items. </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1822:Mark members as static", Justification = "<Pending>" )]
    public void IsSingle( IEnumerable<object> items )
    {
        Assert.IsTrue( items is object );
        Assert.IsTrue( items.Any() );
        Assert.AreEqual( 1, items.Count() );
    }

    /// <summary> Is single. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <param name="items">  The items. </param>
    /// <param name="format"> Describes the format to use. </param>
    /// <param name="args">   A variable-length parameters list containing arguments. </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1822:Mark members as static", Justification = "<Pending>" )]
    public void IsSingle( IEnumerable<object> items, string format, params object[] args )
    {
        Assert.IsTrue( items is object, string.Format( System.Globalization.CultureInfo.CurrentCulture, format, args ) );
        Assert.IsTrue( items.Any(), string.Format( System.Globalization.CultureInfo.CurrentCulture, format, args ) );
        Assert.AreEqual( 1, items.Count(), string.Format( System.Globalization.CultureInfo.CurrentCulture, format, args ) );
    }

    #endregion

    #region " is empty "

    /// <summary> Is single. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <param name="items"> The items. </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1822:Mark members as static", Justification = "<Pending>" )]
    public void IsEmpty( IEnumerable<object> items )
    {
        if ( items is object )
        {
            Assert.IsFalse( items.Any() );
        }
    }

    /// <summary> Is single. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <param name="items">  The items. </param>
    /// <param name="format"> Describes the format to use. </param>
    /// <param name="args">   A variable-length parameters list containing arguments. </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1822:Mark members as static", Justification = "<Pending>" )]
    public void IsEmpty( IEnumerable<object> items, string format, params object[] args )
    {
        if ( items is object )
        {
            Assert.IsFalse( items.Any(), string.Format( System.Globalization.CultureInfo.CurrentCulture, format, args ) );
        }
    }

    #endregion

    #region " throws "

    /// <summary>   Throws an exception to verify that an exception has been throw. </summary>
    /// <remarks>   David, 2020-09-18. </remarks>
    /// <exception cref="AssertFailedException">    Thrown when an Assert Failed error condition
    ///                                             occurs. </exception>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    /// <param name="function"> The function. </param>
    /// <returns>   A T. </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1822:Mark members as static", Justification = "<Pending>" )]
    public T Throws<T>( Action function ) where T : Exception
    {
        T? result = null;
        try
        {
            function?.Invoke();
        }
        catch ( T ex )
        {
            result = ex;
        }

        return result is null
            ? throw new AssertFailedException( $"An exception of type {typeof( T )} was expected, but not thrown" )
            : result;
    }

    /// <summary>   Throws an exception to verify that an exception has been throw. </summary>
    /// <remarks>   David, 2020-09-18. </remarks>
    /// <exception cref="AssertFailedException">    Thrown when an Assert Failed error condition
    ///                                             occurs. </exception>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    /// <param name="function"> The function. </param>
    /// <param name="message">  The message. </param>
    /// <returns>   A T. </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1822:Mark members as static", Justification = "<Pending>" )]
    public T Throws<T>( Action function, string message ) where T : Exception
    {
        T? result = null;
        try
        {
            function?.Invoke();
        }
        catch ( T ex )
        {
            result = ex;
        }

        return result is null
            ? throw new AssertFailedException( $"An exception of type {typeof( T )} was expected, but not thrown; {message}" )
            : result;
    }

    /// <summary> Throws an exception to verify that an exception has been throw. </summary>
    /// <remarks> David, 2020-09-18. </remarks>
    /// <param name="function">   The function. </param>
    /// <param name="format"> Describes the format to use. </param>
    /// <param name="args">   A variable-length parameters list containing arguments. </param>
    /// <returns> A T. </returns>
    public T Throws<T>( Action function, string format, params object[] args ) where T : Exception
    {
        return this.Throws<T>( function, string.Format( System.Globalization.CultureInfo.CurrentCulture, format, args ) );
    }

    #endregion
}
