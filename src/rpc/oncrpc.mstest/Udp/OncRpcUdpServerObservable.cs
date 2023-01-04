using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace cc.isr.ONC.RPC.MSTest.Udp;

public partial class OncRpcUdpServer : INotifyPropertyChanged
{
    /// <summary>   Occurs when a property value changes. </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>   Executes the 'property changed' action. </summary>
    /// <remarks>   2022-10-08. </remarks>
    /// <param name="propertyName"> Name of the property. </param>
    protected virtual void OnPropertyChanged( string propertyName )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    /// <summary>   Executes the 'property changed' action. </summary>
    /// <remarks>   2022-10-08. </remarks>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    /// <param name="backingField"> [in,out] The backing field. </param>
    /// <param name="value">        The value. </param>
    /// <param name="propertyName"> (Optional) Name of the property. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    protected virtual bool OnPropertyChanged<T>( ref T backingField, T value, [CallerMemberName] string propertyName = "" )
    {
        if ( EqualityComparer<T>.Default.Equals( backingField, value ) )
            return false;

        backingField = value;
        this.OnPropertyChanged( propertyName );
        return true;
    }

    /// <summary>   Sets a property. </summary>
    /// <remarks>   2022-12-16. </remarks>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    /// <param name="prop">         [in,out] The property. </param>
    /// <param name="value">        The value. </param>
    /// <param name="propertyName"> (Optional) Name of the property. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    protected bool SetProperty<T>( ref T prop, T value, [CallerMemberName] string propertyName = null )
    {
        if ( object.Equals( prop, value ) ) return false;
        prop = value;
        this.OnPropertyChanged( propertyName );
        return true;
    }

}
