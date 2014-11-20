namespace Spatial.Data.Importer
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

    using DotSpatial.Data;
    using DotSpatial.Topology;
    using DotSpatial.Topology.Utilities;

    using GeoNames.Data;

    using TimezoneConverter;

    using TimeZoneInfo = GeoNames.Data.TimeZoneInfo;

    /// <summary>The data importer.</summary>
    public class DataImporter
    {
        #region Fields

        /// <summary>The connection.</summary>
        private readonly SqlConnection connection;

        #endregion

        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="DataImporter" /> class.</summary>
        public DataImporter()
            : this(new SqlConnection(DataConfiguration.ConnectionString))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DataImporter"/> class.</summary>
        /// <param name="connection">The connection.</param>
        public DataImporter(string connection)
            : this(new SqlConnection(connection))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DataImporter"/> class.</summary>
        /// <param name="connection">The connection.</param>
        protected DataImporter(SqlConnection connection)
        {
            this.connection = connection;
            this.connection.Open();
        }

        #endregion

        #region Methods

        /// <summary>Crea la tabla con la información de los paises.</summary>
        /// <param name="scriptPath">Ruta del script de salida.</param>
        public void GenerateScriptForCountries(string scriptPath = "CountryInfo.sql")
        {
            if (scriptPath == null)
            {
                throw new ArgumentNullException("scriptPath");
            }

            StringBuilder buffer = new StringBuilder();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Debug.Flush();
            buffer.Append(@"SET NOCOUNT ON
                            SET QUOTED_IDENTIFIER ON
                            IF OBJECT_ID('dbo.CountryInfo', 'U') IS NOT NULL
                                BEGIN
                                    DROP TABLE dbo.CountryInfo 
                            END").AppendLine().AppendLine("GO").AppendLine();
            buffer.Append(@"CREATE TABLE dbo.CountryInfo
                            (
                                ISO VARCHAR(2) NOT NULL ,
                                ISO3 VARCHAR(3) NULL ,
                                ISONumeric VARCHAR(3) NULL ,
                                Fips VARCHAR(2) NULL ,
                                Country VARCHAR(50) NULL ,
                                Capital VARCHAR(30) NULL ,
                                Area REAL NULL ,
                                Population REAL NULL ,
                                Continent VARCHAR(2) NULL ,
                                TopLevelDomain VARCHAR(3) NULL ,
                                CurrencyCode VARCHAR(3) NULL ,
                                CurrencyName VARCHAR(20) NULL ,
                                Phone VARCHAR(20) NULL ,
                                PostalCodeFormat VARCHAR(100) NULL ,
                                PostalCodeRegex VARCHAR(200) NULL ,
                                Languages VARCHAR(100) NULL ,
                                GeonameId VARCHAR(10) NULL ,
                                Neighbours VARCHAR(50) NULL ,
                                EquivalentFipsCode VARCHAR(2) NULL,
                                CONSTRAINT [PK_CountryInfo] PRIMARY KEY CLUSTERED ( [ISO] ASC )
                            )").AppendLine().AppendLine("GO").AppendLine();

            IEnumerable<CountryInfo> countries = GeoNamesProvider.GetCountries();
            int cnt = 0;
            foreach (CountryInfo country in countries)
            {
                buffer.AppendFormat(
                    @"INSERT INTO CountryInfo (ISO, ISO3, ISONumeric, Fips, Country, Capital, Area, Population, Continent, TopLevelDomain, CurrencyCode, CurrencyName, Phone, PostalCodeFormat, PostalCodeRegex, Languages, GeonameId, Neighbours, EquivalentFipsCode ) 
                                VALUES('{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}','{9}','{10}','{11}','{12}','{13}', '{14}','{15}', '{16}','{17}','{18}')",
                    country.Iso,
                    country.Iso3,
                    country.IsoNumeric,
                    country.Fips,
                    country.Country,
                    country.Capital.Replace("'", "´"),
                    country.Area.HasValue ? string.Format(CultureInfo.InvariantCulture, "{0}", country.Area) : "null",
                    country.Population.HasValue
                        ? string.Format(CultureInfo.InvariantCulture, "{0}", country.Population)
                        : "null",
                    country.Continent,
                    country.TopLevelDomain,
                    country.CurrencyCode,
                    country.CurrencyName.Replace("'", "´"),
                    country.Phone,
                    country.PostalCodeFormat,
                    country.PostalCodeRegex,
                    country.Languages,
                    country.GeonameId,
                    country.Neighbours,
                    country.EquivalentFipsCode).AppendLine();
                cnt++;
            }

            buffer.AppendLine().AppendLine("GO").AppendLine();
            this.SaveAndExecute(buffer, scriptPath);
            sw.Stop();
            Debug.WriteLine("Imported {0} countries in {1:N1} seconds.", cnt, sw.ElapsedMilliseconds / 1000D);
        }

        /// <summary>Importa los shapes de los timezones y les agrega información adicional.</summary>
        /// <param name="shapePath">The shape Path.</param>
        /// <param name="scriptPath">The script Path.</param>
        public void GenerateScriptForTimeZones(string shapePath, string scriptPath = "TimeZones.sql")
        {
            if (shapePath == null)
            {
                throw new ArgumentNullException("shapePath");
            }

            if (scriptPath == null)
            {
                throw new ArgumentNullException("scriptPath");
            }

            StringBuilder buffer = new StringBuilder();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Debug.Flush();

            buffer.Append(@"SET NOCOUNT ON
                            SET QUOTED_IDENTIFIER ON
                            GO
                            IF OBJECT_ID('dbo.TimeZones', 'U') IS NOT NULL
                                BEGIN
                                    DROP TABLE dbo.TimeZones
                                END
                            GO").AppendLine();

            buffer.Append(@"CREATE TABLE dbo.TimeZones
                            (
                                Id [int] IDENTITY(1, 1) NOT NULL ,
                                TimeZoneId VARCHAR(40) NOT NULL ,
                                TimeZoneName VARCHAR(40) NULL ,
                                TimeZoneDaylightName VARCHAR(40) NULL ,
                                Shape VARCHAR(MAX) NOT NULL ,
                                CountryCode VARCHAR(3) NULL ,
                                GmtOffset DECIMAL(9, 6) NULL ,
                                DstOffset DECIMAL(9, 6) NULL ,
                                RawOffset DECIMAL(9, 6) NULL ,
                                CONSTRAINT [PK_TimeZones] PRIMARY KEY CLUSTERED ( [Id] ASC )
                            )
                            GO").AppendLine();

            int cnt = 0;

            using (IFeatureSet fs = FeatureSet.Open(shapePath))
            {
                WktWriter writer = new WktWriter();
                int numRows = fs.NumRows();
                for (int i = 0; i < numRows; i++)
                {
                    Shape shape = fs.GetShape(i, false);
                    string timeZoneId = (string)shape.Attributes[0];

                    if (string.IsNullOrWhiteSpace(timeZoneId)
                        || timeZoneId.Equals("uninhabited", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Get the shape as a geometry.
                    IGeometry geometry = shape.ToGeometry();

                    // Simplify the geometry.
                    /*
                    IGeometry simplified = null;
                    if (geometry.Area < 0.1)
                    {
                        // For very small regions, use a convex hull.
                        simplified = geometry.ConvexHull();
                    }
                    else
                    {
                        // Simplify the polygon if necessary. Reduce the tolerance incrementally until we have a valid polygon.
                        double tolerance = 0.05;

                        // ReSharper disable RedundantComparisonWithNull
                        while (simplified == null || !(simplified is Polygon) || !simplified.IsValid
                               || simplified.IsEmpty)
                        {
                            // ReSharper restore RedundantComparisonWithNull
                            simplified = TopologyPreservingSimplifier.Simplify(geometry, tolerance);
                            tolerance -= 0.005;
                        }
                    }
                    */
                    IGeometry simplified = geometry;

                    // Convert it to WKT.
                    string shapeWkt = writer.Write((Geometry)simplified);
                    string timeZoneName = TimezoneConverterProvider.OlsonToWindows(timeZoneId);
                    string timeZoneDaylightName = TimezoneConverterProvider.OlsonToWindows(timeZoneId, true);
                    TimeZoneInfo geoname = GeoNamesProvider.GetTimeZoneInfo(timeZoneId);
                    bool hasValue = geoname != null;

                    if (hasValue)
                    {
                        buffer.AppendFormat(
                            CultureInfo.InvariantCulture,
                            @"INSERT INTO TimeZones (TimeZoneId, TimeZoneName, TimeZoneDaylightName, Shape, CountryCode, GmtOffset, DstOffset, RawOffset) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', {5}, {6}, {7})",
                            timeZoneId,
                            timeZoneName,
                            timeZoneDaylightName,
                            shapeWkt,
                            geoname.CountryCode,
                            geoname.GmtOffset,
                            geoname.DstOffset,
                            geoname.RawOffset);
                    }
                    else
                    {
                        buffer.AppendFormat(
                            CultureInfo.InvariantCulture,
                            @"INSERT INTO TimeZones (TimeZoneId, TimeZoneName, TimeZoneDaylightName, Shape) VALUES ('{0}', '{1}', '{2}', '{3}')",
                            timeZoneId,
                            timeZoneName,
                            timeZoneDaylightName,
                            shapeWkt);
                    }

                    buffer.AppendLine();

                    cnt++;
                }
            }

            buffer.Append("GO").AppendLine();
            buffer.Append(@"ALTER TABLE dbo.TimeZones ADD [GeoData] GEOMETRY NULL")
                .AppendLine()
                .AppendLine("GO")
                .AppendLine();
            buffer.Append(@"UPDATE dbo.TimeZones SET [GeoData] = geometry::STGeomFromText( Shape, 4326 )")
                .AppendLine()
                .AppendLine("GO")
                .AppendLine();
            buffer.Append(@"ALTER TABLE dbo.TimeZones DROP Shape").AppendLine().AppendLine("GO").AppendLine();
            buffer.Append(
                @"CREATE SPATIAL INDEX ix_geo ON dbo.TimeZones( [GeoData] )USING GEOMETRY_GRID WITH ( BOUNDING_BOX =(-90, -180, 90, 180))")
                .AppendLine()
                .AppendLine("GO")
                .AppendLine();
            buffer.AppendLine();

            this.SaveAndExecute(buffer, scriptPath);

            sw.Stop();
            Debug.WriteLine("Imported {0} shapes in {1:N1} seconds.", cnt, sw.ElapsedMilliseconds / 1000D);
        }

        #endregion

        #region Methods

        private void SaveAndExecute(StringBuilder buffer, string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                using (StreamWriter streamWriter = new StreamWriter(File.Create(file)))
                {
                    streamWriter.Write(buffer.ToString());
                }
            }
            catch (Exception exception)
            {
            }

            try
            {
                using (SqlTransaction txn = this.connection.BeginTransaction())
                {
                    using (SqlCommand cmd = this.connection.CreateCommand())
                    {
                        cmd.Transaction = txn;
                        cmd.CommandText = buffer.ToString();
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
            catch (Exception exception)
            {
            }
        }

        #endregion
    }
}