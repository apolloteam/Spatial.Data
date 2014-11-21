namespace Spatial.Data.Countries
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;

    /// <summary>The country info field.</summary>
    internal enum CountryInfoField
    {
        /// <summary>The iso.</summary>
        Iso = 0, 

        /// <summary>The is o 3.</summary>
        Iso3 = 1, 

        /// <summary>The iso numeric.</summary>
        IsoNumeric = 2, 

        /// <summary>The fips.</summary>
        Fips = 3, 

        /// <summary>The country.</summary>
        Country = 4, 

        /// <summary>The capital area.</summary>
        Capital = 5, 

        /// <summary>The area.</summary>
        Area = 6, 

        /// <summary>The population.</summary>
        Population = 7, 

        /// <summary>The continent.</summary>
        Continent = 8, 

        /// <summary>Top Level Domain.</summary>
        TopLevelDomain = 9, 

        /// <summary>The currency code.</summary>
        CurrencyCode = 10, 

        /// <summary>The currency name.</summary>
        CurrencyName = 11, 

        /// <summary>The phone.</summary>
        Phone = 12, 

        /// <summary>The postal code format.</summary>
        PostalCodeFormat = 13, 

        /// <summary>The postal code regex.</summary>
        PostalCodeRegex = 14, 

        /// <summary>The languages.</summary>
        Languages = 15, 

        /// <summary>The geoname id.</summary>
        GeonameId = 16, 

        /// <summary>The neighbours.</summary>
        Neighbours = 17, 

        /// <summary>The equivalent fips code.</summary>
        EquivalentFipsCode = 18
    }

    /// <summary>The country info provider.</summary>
    public class CountryInfoProvider
    {
        #region Constants

        /// <summary>The query sql.</summary>
        private const string QuerySqlFirst =
            @"DECLARE @p AS GEOMETRY = GEOMETRY::STGeomFromText('POINT( {1} {0} )', 4326)
                                          SELECT c.* 
                                          FROM dbo.CountryInfo c (NOLOCK)
                                          INNER JOIN dbo.TimeZones t (NOLOCK) ON c.ISO = t.CountryCode 
                                          WHERE t.GeoData.STContains ( @p ) = 1";

        /// <summary>The query sql other.</summary>
        private const string QuerySqlOther =
            @"DECLARE @p AS GEOMETRY = GEOMETRY::STGeomFromText('POINT( {1} {0} )', 4326)
                                          SELECT c.* 
                                          FROM dbo.CountryInfo c (NOLOCK)
                                          INNER JOIN dbo.TimeZones t (NOLOCK) ON c.ISO = t.CountryCode 
                                          WHERE t.GeoData.STDistance( @p ) < 1
                                          ORDER BY t.GeoData.STDistance( @p )";

        #endregion

        #region Fields

        /// <summary>The connection.</summary>
        private readonly string connection;

        /// <summary>The sync lock.</summary>
        private readonly object syncLock = new object();

        /// <summary>The map country info.</summary>
        private IDictionary<string, CountryInfo> mapCountryInfo;

        #endregion

        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="CountryInfoProvider" /> class.</summary>
        public CountryInfoProvider()
            : this(DataConfiguration.ConnectionString)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CountryInfoProvider"/> class.</summary>
        /// <param name="connection">The connection.</param>
        public CountryInfoProvider(string connection)
        {
            this.connection = connection;
        }

        #endregion

        #region Properties

        /// <summary>Gets the map country info.</summary>
        private IDictionary<string, CountryInfo> MapCountryInfo
        {
            get
            {
                if (this.mapCountryInfo == null)
                {
                    lock (this.syncLock)
                    {
                        if (this.mapCountryInfo == null)
                        {
                            this.mapCountryInfo = new ConcurrentDictionary<string, CountryInfo>();
                            using (IDbConnection conn = new SqlConnection(this.connection))
                            {
                                conn.Open();
                                using (IDbCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = @"SELECT * FROM dbo.CountryInfo;";
                                    cmd.Prepare();
                                    using (IDataReader dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            CountryInfo country = this.FillEntity(dr);
                                            this.mapCountryInfo.Add(country.ISO, country);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return this.mapCountryInfo;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>The get country.</summary>
        /// <param name="countryCode">The country code.</param>
        /// <returns>The <see cref="RunQuery"/>.</returns>
        public CountryInfo GetCountry(string countryCode)
        {
            CountryInfo country;
            this.MapCountryInfo.TryGetValue(countryCode, out country);
            return country;
        }

        /// <summary>The get country.</summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>The <see cref="RunQuery"/>.</returns>
        public CountryInfo GetCountry(decimal latitude, decimal longitude)
        {
            CountryInfo country = null;
            using (IDbConnection conn = new SqlConnection(this.connection))
            {
                conn.Open();
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    string sql = string.Format(CultureInfo.InvariantCulture, QuerySqlFirst, latitude, longitude);
                    country = this.RunQuery(cmd, sql);
                    if (country == null)
                    {
                        sql = string.Format(CultureInfo.InvariantCulture, QuerySqlOther, latitude, longitude);
                        country = this.RunQuery(cmd, sql);
                    }
                }
            }

            return country;
        }

        #endregion

        #region Methods

        /// <summary>The fill entity.</summary>
        /// <param name="dr">The dr.</param>
        /// <returns>The <see cref="RunQuery"/>.</returns>
        private CountryInfo FillEntity(IDataReader dr)
        {
            object valueAux;
            CountryInfo country = new CountryInfo();
            country.ISO = dr.GetString((int)CountryInfoField.Iso);
            country.ISO3 = dr.GetString((int)CountryInfoField.Iso3);
            country.ISONumeric = dr.GetString((int)CountryInfoField.IsoNumeric);
            country.Fips = dr.GetString((int)CountryInfoField.Fips);
            country.Country = dr.GetString((int)CountryInfoField.Country);
            country.Capital = dr.GetString((int)CountryInfoField.Capital);
            valueAux = dr.GetValue((int)CountryInfoField.Area);
            country.Area = valueAux == DBNull.Value ? null : (decimal?)Convert.ToDecimal(valueAux);
            valueAux = dr.GetValue((int)CountryInfoField.Population);
            country.Population = valueAux == DBNull.Value ? null : (decimal?)Convert.ToDecimal(valueAux);
            country.Continent = dr.GetString((int)CountryInfoField.Continent);
            country.TopLevelDomain = dr.GetString((int)CountryInfoField.TopLevelDomain);
            country.CurrencyCode = dr.GetString((int)CountryInfoField.CurrencyCode);
            country.CurrencyName = dr.GetString((int)CountryInfoField.CurrencyName);
            country.Phone = dr.GetString((int)CountryInfoField.Phone);
            country.PostalCodeFormat = dr.GetString((int)CountryInfoField.PostalCodeFormat);
            country.PostalCodeRegex = dr.GetString((int)CountryInfoField.PostalCodeRegex);
            string langsAux = dr.GetString((int)CountryInfoField.Languages);
            string[] langs = langsAux.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string lang in langs)
            {
                country.Languages.Add(lang);
            }

            country.EquivalentFipsCode = dr.GetString((int)CountryInfoField.EquivalentFipsCode);
            return country;
        }

        /// <summary>The run query.</summary>
        /// <param name="cmd">The cmd.</param>
        /// <param name="sql">The sql.</param>
        /// <returns>The <see cref="CountryInfo"/>.</returns>
        private CountryInfo RunQuery(IDbCommand cmd, string sql)
        {
            CountryInfo country = null;
            cmd.CommandText = sql;
            cmd.Prepare();
            using (IDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    country = this.FillEntity(dr);
                }
            }

            return country;
        }

        #endregion
    }
}