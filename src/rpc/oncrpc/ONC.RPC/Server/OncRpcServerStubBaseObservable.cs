using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace cc.isr.ONC.RPC.Server;

public partial class OncRpcServerStubBase : INotifyPropertyChanged
{
    /// <summary>   Occurs when a property value changes. </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>   Executes the 'property changed' action. </summary>
    /// <param name="propertyName"> Name of the property. </param>
    protected virtual void OnPropertyChanged( string? propertyName )
    {
        if ( !string.IsNullOrEmpty( propertyName ) )
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    /// <summary>   Executes the 'property changed' action. </summary>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    /// <param name="backingField"> [in,out] The backing field. </param>
    /// <param name="value">        The value. </param>
    /// <param name="propertyName"> (Optional) Name of the property. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    protected virtual bool OnPropertyChanged<T>( ref T backingField, T value, [CallerMemberName] string? propertyName = null )
    {
        if ( EqualityComparer<T>.Default.Equals( backingField, value ) )
            return false;

        backingField = value;
        this.OnPropertyChanged( propertyName );
        return true;
    }

    /// <summary>   Sets a property. </summary>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    /// <param name="prop">         [in,out] The property. </param>
    /// <param name="value">        The value. </param>
    /// <param name="propertyName"> (Optional) Name of the property. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    protected bool SetProperty<T>( ref T prop, T value, [CallerMemberName] string? propertyName = null )
    {
        if ( EqualityComparer<T>.Default.Equals( prop, value ) ) return false;
        prop = value;
        this.OnPropertyChanged( propertyName );
        return true;
    }

    /// <summary>
    /// Compares the current and new values for a given nested property. If the value has changed,
    /// updates the property and then raises the <see cref="PropertyChanged"/> event. 
    /// The behavior mirrors that of <see cref="SetProperty{T}(ref T,T,string)"/>,
    /// with the difference being that this method is used to relay properties from a wrapped model in the
    /// current instance. This type is useful when creating wrapping, bindable objects that operate over
    /// models that lack support for notification (e.g., for CRUD operations).
    /// Suppose we have this model (eg. for a database row in a table):
    /// <code>
    /// public class Person
    /// {
    ///     public string Name { get; set; }
    /// }
    /// </code>
    /// We can then use a property to wrap instances of this type into our observable model (which supports
    /// notifications), injecting the notification to the properties of that model, like so:
    /// <code>
    /// public class BindablePerson : ObservableObject
    /// {
    ///     public Model { get; }
    ///
    ///     public BindablePerson(Person model)
    ///     {
    ///         Model = model;
    ///     }
    ///
    ///     public string Name
    ///     {
    ///         get => Model.Name;
    ///         set => Set(Model.Name, value, Model, (model, name) => model.Name = name);
    ///     }
    /// }
    /// </code>
    /// This way we can then use the wrapping object in our application, and all those "proxy" properties will
    /// also raise notifications when changed. Note that this method is not meant to be a replacement for
    /// <see cref="SetProperty{T}(ref T,T,string)"/>, and it should only be used when relaying properties to a model that
    /// doesn't support notifications, and only if you can't implement notifications to that model directly (e.g., by having
    /// it inherit from an 'ObservableObject'. The syntax relies on passing the target model and a stateless callback
    /// to allow the C# compiler to cache the function, which results in much better performance and no memory usage.
    /// </summary>
    /// <typeparam name="TModel">The type of model whose property (or field) to set.</typeparam>
    /// <typeparam name="T">The type of property (or field) to set.</typeparam>
    /// <param name="oldValue">The current property value.</param>
    /// <param name="newValue">The property's value after the change occurred.</param>
    /// <param name="model">The model containing the property being updated.</param>
    /// <param name="callback">The callback to invoke to set the target property value, if a change has occurred.</param>
    /// <param name="propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// The <see cref="PropertyChanged"/> event is not raised if the current and new value for the target property are the same.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="model"/> or <paramref name="callback"/> are (<see langword="null"/>).</exception>
    protected bool SetProperty<TModel, T>( T oldValue, T newValue, TModel model, Action<TModel, T> callback, [CallerMemberName] string? propertyName = null )
        where TModel : class
    {
        if ( model is null ) throw new ArgumentNullException( nameof( model ) );
        if ( callback is null ) throw new ArgumentNullException( nameof( callback ) );

        if ( EqualityComparer<T>.Default.Equals( oldValue, newValue ) )
        {
            return false;
        }

        callback( model, newValue );

        this.OnPropertyChanged( propertyName );

        return true;
    }

    protected bool SetProperty<T>( T oldValue, T newValue, Action callback, [CallerMemberName] string? propertyName = null )
    {
        if ( callback is null ) throw new ArgumentNullException( nameof( callback ) );

        if ( EqualityComparer<T>.Default.Equals( oldValue, newValue ) )
        {
            return false;
        }

        callback();

        this.OnPropertyChanged( propertyName );

        return true;
    }

}
