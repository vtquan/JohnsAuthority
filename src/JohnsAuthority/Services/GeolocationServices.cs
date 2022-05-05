using JohnsAuthority.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace JohnsAuthority.Services
{
    public class GeolocationServices
    {
        private readonly ILogger _logger;
        public GeolocationServices(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GeolocationServices>();
        }

        public static async Task<Coordinate> Geocode(string address)
        {
            address = address.Replace(" ", "%20");
            var requestUri = "https://maps.googleapis.com/maps/api/geocode/json?address=" + address + "&key=###";
            try
            {
                var request = WebRequest.Create(requestUri);
                var response = await request.GetResponseAsync();
                var streamReader = new StreamReader(response.GetResponseStream());
                var jsonString = streamReader.ReadToEnd();
                dynamic root = JsonConvert.DeserializeObject<GoogleGeocodeResult>(jsonString);
                return root.results[0].geometry.location;
            }
            catch
            {
                return new Coordinate { lat = 0, lng = 0 };
            }
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        // Calculate distance between two coordinate in km. Taken from Marcell Spies, with translation by Michael List
        public static double CalculateDistanceInKilometers(Coordinate location1, Coordinate location2)
        {
            double circumference = 40000.0; // Earth's circumference at the equator in km
            double distance = 0.0;

            //Calculate radians
            double latitude1Rad = DegreesToRadians(location1.lat);
            double longitude1Rad = DegreesToRadians(location1.lng);
            double latititude2Rad = DegreesToRadians(location2.lat);
            double longitude2Rad = DegreesToRadians(location2.lng);

            double logitudeDiff = Math.Abs(longitude1Rad - longitude2Rad);

            if (logitudeDiff > Math.PI)
            {
                logitudeDiff = 2.0 * Math.PI - logitudeDiff;
            }

            double angleCalculation =
                Math.Acos(
                  Math.Sin(latititude2Rad) * Math.Sin(latitude1Rad) +
                  Math.Cos(latititude2Rad) * Math.Cos(latitude1Rad) * Math.Cos(logitudeDiff));

            distance = circumference * angleCalculation / (2.0 * Math.PI);

            return distance;
        }

        public static double CalculateDistanceInMiles(Coordinate location1, Coordinate location2)
        {
            var distance = CalculateDistanceInKilometers(location1, location2);

            return ConvertKilometerToMile(distance);
        }

        public static double ConvertKilometerToMile(double distance)
        {
            return distance * 0.621371;
        }

        public static double ConvertMileToKilometer(double distance)
        {
            return distance * 1.609344;
        }

        public static double ConvertMileToMeter(double distance)
        {
            return distance * 1.609344 * 1000;
        }
    }
}
