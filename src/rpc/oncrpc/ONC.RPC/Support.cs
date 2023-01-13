using System.ComponentModel;
using System.Reflection;

namespace cc.isr.ONC.RPC
{
    internal class Support
    {

        /// <summary>   Gets a description from an Enum. </summary>
        /// <remarks>   2023-01-07. </remarks>
        /// <param name="value">    An enum constant representing the value option. </param>
        /// <returns>   The description. </returns>
        public static string GetDescription( Enum value )
        {
            return
                value
                    .GetType()
                    .GetMember( value.ToString() )
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description
                ?? value.ToString();
        }
    }
}
