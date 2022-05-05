using JohnsAuthority.Data;
using JohnsAuthority.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yelp.Api.Web.Models;

namespace JohnsAuthority.Models.LocationViewModels
{
    public class LocationDetailViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public Hour[] Hours { get; set; }
        public Coordinate Coordinate { get; set; }
        public PaginatedList<Review> Reviews { get; set; }
        public ICollection<Image> Images { get; set; }
        [Display(Name = "Amenities")]
        public List<LocationAmenity> LocationAmenities { get; set; }

        public LocationDetailViewModel()
        {
            Images = new List<Image>();
            LocationAmenities = new List<LocationAmenity>();
        }

        public LocationDetailViewModel(BusinessResponse business, ApplicationDbContext _context, int? page = 1, int pageSize = 10)
        {
            Id = business.Id;
            Name = business.Name;
            Phone = business.Phone;
            Address1 = business.Location.Address1;
            Address2 = business.Location.Address2;
            Address3 = business.Location.Address3;
            City = business.Location.City;
            State = business.Location.State;
            ZipCode = business.Location.ZipCode;
            Hours = business.Hours;
            Coordinate = new Coordinate() { lat = business.Coordinates.Latitude, lng = business.Coordinates.Longitude };
            
            var location = _context.Location
                .Include(m => m.Reviews)
                    .ThenInclude(m => m.User)
                .Include(m => m.Images)
                .SingleOrDefault(m => String.Equals(m.Id, business.Id));

            if(location != null)
            {
                Reviews = PaginatedList<Review>.Create(location.Reviews.OrderByDescending(m => m.Date).AsQueryable(), page ?? 1, pageSize);
                Images = location.Images;
                LocationAmenities = location.LocationAmenities;
            }
            else
            {
                Images = new List<Image>();
                LocationAmenities = new List<LocationAmenity>();
            }
        }

        public LocationDetailViewModel(BusinessResponse business, Location location, int? page = 1, int pageSize = 10)
        {
            Id = business.Id;
            Name = business.Name;
            Phone = business.Phone;
            Address1 = business.Location.Address1;
            Address2 = business.Location.Address2;
            Address3 = business.Location.Address3;
            City = business.Location.City;
            State = business.Location.State;
            ZipCode = business.Location.ZipCode;
            Hours = business.Hours;
            Coordinate = new Coordinate() { lat = business.Coordinates.Latitude, lng = business.Coordinates.Longitude };
            Reviews = PaginatedList<Review>.Create(location.Reviews.OrderByDescending(m => m.Date).AsQueryable(), page ?? 1, pageSize);
            Images = location.Images;
            LocationAmenities = location.LocationAmenities;
        }

        public float GetRating()
        {
            if ((Reviews?.Count() ?? 0) == 0)
            {
                return -1;
            }
            var sumScore = Reviews.Sum(r => r.Score);
            float averageScore = sumScore / Reviews.Count();
            return averageScore;
        }

        public string PrintAddress()
        {
            if (String.IsNullOrEmpty(Address2))
                return $"{Address1} {City}, {State} {ZipCode}";
            else if (String.IsNullOrEmpty(Address3))
                return $"{Address1} {Address2} {City}, {State} {ZipCode}";
            else
                return $"{Address1} {Address2} {Address3} {City}, {State} {ZipCode}";
        }

        public string MapLink()
        {
            var address = "";
            if (String.IsNullOrEmpty(Address2))
                address = $"{Address1} {City}, {State} {ZipCode}";
            else
                address = $"{Address1} {Address2} {City}, {State} {ZipCode}";

            address = address.Replace(" ", "+");

            var url = $"http://maps.apple.com/?q={address}";

            return url;
        }

        public string PrintAmenities()
        {
            var sb = new StringBuilder();
            foreach (var locationAmenity in LocationAmenities)
            {
                if (locationAmenity == LocationAmenities.Last())
                {
                    sb.Append($"{locationAmenity.Amenity.Name}");
                }
                else
                {
                    sb.Append($"{locationAmenity.Amenity.Name} | ");
                }
            }

            return sb.ToString();
        }

        public string PrintPhone()
        {
            string countryCode;
            string areaCode;
            string exchange;
            string number;

            if (Phone.Length == 10)
            {
                areaCode = Phone.Substring(0, 3);
                exchange = Phone.Substring(3, 3);
                number = Phone.Substring(6);

                return $"({areaCode}) {exchange}-{number}";
            }
            else if (Phone.Length == 11)
            {
                countryCode = Phone.Substring(0, 1);
                areaCode = Phone.Substring(1, 3);
                exchange = Phone.Substring(4, 3);
                number = Phone.Substring(7);

                return $"+{countryCode} ({areaCode}) {exchange}-{number}";
            }
            else if (Phone.Length == 12)
            {
                countryCode = Phone.Substring(1, 1);
                areaCode = Phone.Substring(2, 3);
                exchange = Phone.Substring(5, 3);
                number = Phone.Substring(8);

                return $"+{countryCode} ({areaCode}) {exchange}-{number}";
            }
            else
                return Phone;
        }

        public ICollection<Amenity> GetAmenties()
        {
            return LocationAmenities.Select(m => m.Amenity).ToList();
        }
    }
}
