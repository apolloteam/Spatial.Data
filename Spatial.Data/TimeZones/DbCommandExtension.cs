namespace Spatial.Data.TimeZones
{
    using System;
    using System.Data;

    /// <summary>The db command extension.</summary>
    internal static class DbCommandExtension
    {
        #region Methods

        /// <summary>Ejecuta una consulta y carga los datos.</summary>
        /// <param name="cmd">DbCommand extendido.</param>
        /// <param name="sql">Consulta sql.</param>
        /// <returns>objeto timezone.<see cref="TimeZoneInfo"/></returns>
        /// <exception cref="ArgumentNullException">Argumentos nulos.</exception>
        public static TimeZoneInfo RunQuery(this IDbCommand cmd, string sql)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }

            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            TimeZoneInfo tz = null;
            cmd.CommandText = sql;
            cmd.Prepare();
            using (IDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    tz = new TimeZoneInfo();
                    tz.CountryCode = dr.GetString((int)TimeZonesFields.CountryCode);
                    tz.TimeZoneId = dr.GetString((int)TimeZonesFields.TimeZoneId);
                    tz.TimeZoneName = dr.GetString((int)TimeZonesFields.TimeZoneName);
                    tz.TimeZoneDaylightName = dr.GetString((int)TimeZonesFields.TimeZoneDaylightName);
                    object aux = dr.GetValue((int)TimeZonesFields.GmtOffset);
                    tz.GmtOffset = aux == DBNull.Value ? null : (decimal?)Convert.ToDecimal(aux);
                    aux = dr.GetValue((int)TimeZonesFields.DstOffset);
                    tz.DstOffset = aux == DBNull.Value ? null : (decimal?)Convert.ToDecimal(aux);
                    aux = dr.GetValue((int)TimeZonesFields.RawOffset);
                    tz.RawOffset = aux == DBNull.Value ? null : (decimal?)Convert.ToDecimal(aux);
                }
            }

            return tz;
        }

        #endregion
    }
}