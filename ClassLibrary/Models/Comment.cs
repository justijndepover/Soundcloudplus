namespace ClassLibrary.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string CreatedAt { get; set; }
        public int UserId { get; set; }
        public int TrackId { get; set; }
        public int Timestamp { get; set; }
        public string Body { get; set; }
        public string Uri { get; set; }
        public User User { get; set; }
    }
}