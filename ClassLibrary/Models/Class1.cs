using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    class Class1
    {
        public class Rootobject
        {
            public Collection[] collection { get; set; }
            public object next_href { get; set; }
        }

        public class Collection
        {
            public string type { get; set; }
            public User user { get; set; }
            public Playlist playlist { get; set; }
            public string uuid { get; set; }
            public string created_at { get; set; }
        }

        public class User
        {
            public string full_name { get; set; }
            public object country { get; set; }
            public object city { get; set; }
            public int track_count { get; set; }
            public int followers_count { get; set; }
            public int followings_count { get; set; }
            public int public_favorites_count { get; set; }
            public int groups_count { get; set; }
            public object description { get; set; }
            public string plan { get; set; }
            public bool verified { get; set; }
            public int id { get; set; }
            public string uri { get; set; }
            public string username { get; set; }
            public string kind { get; set; }
            public string permalink { get; set; }
            public string permalink_url { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string avatar_url { get; set; }
            public string last_modified { get; set; }
        }

        public class Playlist
        {
            public int id { get; set; }
            public string artwork_url { get; set; }
            public string created_at { get; set; }
            public object description { get; set; }
            public int duration { get; set; }
            public string embeddable_by { get; set; }
            public object genre { get; set; }
            public string kind { get; set; }
            public object label_id { get; set; }
            public object label_name { get; set; }
            public string license { get; set; }
            public int likes_count { get; set; }
            public string permalink { get; set; }
            public bool _public { get; set; }
            public object purchase_title { get; set; }
            public object purchase_url { get; set; }
            public object release { get; set; }
            public object release_date { get; set; }
            public int reposts_count { get; set; }
            public string sharing { get; set; }
            public bool streamable { get; set; }
            public string tag_list { get; set; }
            public string title { get; set; }
            public int track_count { get; set; }
            public User1 user { get; set; }
            public int user_id { get; set; }
            public string last_modified { get; set; }
            public string permalink_url { get; set; }
            public object secret_token { get; set; }
            public string uri { get; set; }
        }

        public class User1
        {
            public int id { get; set; }
            public string uri { get; set; }
            public string username { get; set; }
            public string kind { get; set; }
            public string permalink { get; set; }
            public string permalink_url { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string avatar_url { get; set; }
            public string last_modified { get; set; }
        }

    }
}
