namespace Spatial.Data
{
    using System.Configuration;

    /// <summary>The time zones provider configuration.</summary>
    public static class DataConfiguration
    {
        #region Properties

        /// <summary>Gets the connection string.</summary>
        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["GeoData"]
                                                 ?? ConfigurationManager.ConnectionStrings["Default"];
                return settings.ConnectionString;
            }
        }

        #endregion
    }
}