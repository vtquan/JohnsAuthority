using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JohnsAuthority.Services
{
    public class YelpApi
    {
        public Yelp.Api.Web.Client Client { get; set; }
        
        public YelpApi()
        {
            Client = new Yelp.Api.Web.Client("CCff-aom5xwDJKsbDbe89g", "lq6irvMYDmgZKn2dibxGoSiiHiK9BF3THEH9C5MgzhhgQzBHERDjt2ob2M9qaB4g");
        }
    }
}
