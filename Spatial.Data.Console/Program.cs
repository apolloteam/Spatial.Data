namespace Spatial.Data.Console
{
    using System;
    using System.Diagnostics;

    using Newtonsoft.Json;

    using Spatial.Data.Countries;
    using Spatial.Data.Importer;
    using Spatial.Data.TimeZones;

    using TimeZoneInfo = Spatial.Data.TimeZones.TimeZoneInfo;

    /// <summary>The program.</summary>
    internal class Program
    {
        #region Methods

        /// <summary>The main.</summary>
        /// <param name="args">The args.</param>
        private static void Main(string[] args)
        {
            // ReSharper restore UnusedParameter.Local
            DataImporter di = new DataImporter();
            di.GenerateScriptForTimeZones(@"N:\Data\Local\world\tz_world.shp");
            di.GenerateScriptForCountries();

            // Prueba los archivos importados.
            TimeZonesProvider tzp = new TimeZonesProvider();
            TimeZoneInfo timeZone;
            Stopwatch sw = Stopwatch.StartNew();
            timeZone = tzp.GetTimeZone(-34.6379425M, -58.3756365M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: -34.6379425M, -58.3756365M O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(33.45M, -112.066667M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: 33.45M, -112.066667M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(-24.1931095M, -65.4455425M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: -24.1931095M, -65.4455425M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(-34.6158527M, -58.4332985M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: -34.6158527M, -58.4332985M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(-34.8198798M, -56.2303067M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: -34.8198798M, -56.2303067M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(-34.8198798M, -56.2303067M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: -34.8198798M, -56.2303067M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(-32.9264482M, -68.813779M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: -32.9264482M, -68.813779M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(-26.8285851M, -65.2515487M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: -26.8285851M, -65.2515487M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(-33.6682982M, -70.363372M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: -33.6682982M, -70.363372M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(-41.2443701M, 174.7618546M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: -41.2443701M, 174.7618546M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(40.4378271M, -3.6795367M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: 40.4378271M, -3.6795367M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            timeZone = tzp.GetTimeZone(25.8265645M, -80.229947M);
            sw.Stop();
            Console.WriteLine(
                "D: {0} P: 25.8265645M, -80.229947M D: {0} O: {1}", 
                sw.Elapsed.TotalMilliseconds, 
                JsonConvert.SerializeObject(timeZone));
            sw = Stopwatch.StartNew();

            CountryInfoProvider cip = new CountryInfoProvider();
            CountryInfo country;
            country = cip.GetCountry("AR");
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(-34.6379425M, -58.3756365M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(33.45M, -112.066667M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(-24.1931095M, -65.4455425M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(-34.6158527M, -58.4332985M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(-34.8198798M, -56.2303067M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(-34.8198798M, -56.2303067M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(-32.9264482M, -68.813779M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(-26.8285851M, -65.2515487M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(-33.6682982M, -70.363372M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(-41.2443701M, 174.7618546M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(40.4378271M, -3.6795367M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));

            country = cip.GetCountry(25.8265645M, -80.229947M);
            sw.Stop();
            Console.WriteLine(JsonConvert.SerializeObject(country));
            Console.ReadKey();
        }

        #endregion
    }
}