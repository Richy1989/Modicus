//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Modicus.Resources
{
    
    internal partial class Resources
    {
        private static System.Resources.ResourceManager manager;
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if ((Resources.manager == null))
                {
                    Resources.manager = new System.Resources.ResourceManager("Modicus.Resources.Resources", typeof(Resources).Assembly);
                }
                return Resources.manager;
            }
        }
        internal static string GetString(Resources.StringResources id)
        {
            return ((string)(nanoFramework.Runtime.Native.ResourceUtility.GetObject(ResourceManager, id)));
        }
        [System.SerializableAttribute()]
        internal enum StringResources : short
        {
            select_sensor = -26240,
            mqtt_settings = -21146,
            ip_settings = -11416,
            edit_sensor = -8290,
            style = -5605,
            index = -2501,
            system_settings = 19842,
            i2csettings = 26358,
            head = 31175,
        }
    }
}
