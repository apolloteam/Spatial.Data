namespace Spatial.Data.TimeZones
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>The time zones fields.</summary>
    internal enum TimeZonesFields
    {
        /// <summary>Country code.</summary>
        CountryCode = 0,

        /// <summary>TimeZone Id.</summary>
        TimeZoneId = 1,

        /// <summary>TimeZone name.</summary>
        TimeZoneName = 2,

        /// <summary>TimeZone daylight name.</summary>
        TimeZoneDaylightName = 3,

        /// <summary>Gmt offset.</summary>
        GmtOffset = 4,

        /// <summary>Dst offset.</summary>
        DstOffset = 5,

        /// <summary>Raw offset.</summary>
        RawOffset = 6
    }

    /// <summary>Timezones provider.</summary>
    public class TimeZonesProvider
    {
        #region Constants

        /// <summary>Query que se utiliza para buscar el punto dentro de un shape.</summary>
        private const string QuerySqlFirst = @"SELECT 
                                            CountryCode, 
                                            TimeZoneId, 
                                            TimeZoneName, 
                                            TimeZoneDaylightName, 
                                            GmtOffset, 
                                            DstOffset, 
                                            RawOffset 
                                        FROM dbo.TimeZones t (NOLOCK)
                                        WHERE t.GeoData.STContains ( geometry::STGeomFromText( 'POINT( {1} {0} )', 4326 ) ) = 1;";

        /// <summary>Query que se utiliza para buscar el punto cercano a un shape.</summary>
        private const string QuerySqlOthers = @"DECLARE @p AS GEOMETRY = GEOMETRY::STGeomFromText('POINT( {1} {0} )', 4326)
                                                SELECT 
                                                    CountryCode, 
                                                    TimeZoneId, 
                                                    TimeZoneName, 
                                                    TimeZoneDaylightName, 
                                                    GmtOffset, 
                                                    DstOffset, 
                                                    RawOffset 
                                                FROM dbo.TimeZones t (NOLOCK)
                                                WHERE t.GeoData.STDistance( @p ) < 1
                                                ORDER BY t.GeoData.STDistance( @p )";

        #endregion

        #region Fields

        /// <summary>The connection.</summary>
        private readonly string connection;

        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="TimeZonesProvider" /> class.</summary>
        public TimeZonesProvider()
            : this(DataConfiguration.ConnectionString)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TimeZonesProvider"/> class.</summary>
        /// <param name="connection">The connection.</param>
        public TimeZonesProvider(string connection)
        {
            this.connection = connection;
        }

        #endregion

        #region Methods

        /// <summary>The get time zone.</summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>The <see cref="TimeZoneInfo"/>.</returns>
        public TimeZoneInfo GetTimeZone(decimal latitude, decimal longitude)
        {
            TimeZoneInfo tz = null;
            using (IDbConnection conn = new SqlConnection(this.connection))
            {
                conn.Open();
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    // Ejecuta la consulta buscando que el punto este contenido en un shape.
                    string sql = string.Format(CultureInfo.InvariantCulture, QuerySqlFirst, latitude, longitude);
                    tz = cmd.RunQuery(sql);
                    if (tz == null)
                    {
                        // Ejecuta la consulta buscando que el punto este cerca en un shape.
                        sql = string.Format(CultureInfo.InvariantCulture, QuerySqlOthers, latitude, longitude);
                        tz = cmd.RunQuery(sql);
                    }
                }
            }

            return tz;
        }

        #endregion
    }
}