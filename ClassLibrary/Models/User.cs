using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Permalink { get; set; }
        public string Username { get; set; }
        public string Uri { get; set; }
        public string PermalinkUrl { get; set; }
        public string AvatarUrl { get; set; }
        public string Country { get; set; }
        public string FullName { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        public object DiscogsName { get; set; }
        public object MyspaceName { get; set; }
        public string Website { get; set; }
        public string WebsiteTitle { get; set; }
        public bool Online { get; set; }
        public int TrackCount { get; set; }
        public int PlaylistCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingsCount { get; set; }
        public int PublicFavoritesCount { get; set; }
    }
}