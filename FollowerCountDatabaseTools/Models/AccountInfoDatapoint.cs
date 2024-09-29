namespace FollowerCountDatabaseTools.Models
{
    public class AccountInfoDataPoint
    {
        public string Name { get; }
        public int Followers { get; }
        public int Following { get; }
        public int Posts { get; }
        public DateTime RecordTime { get; }

        public AccountInfoDataPoint(string name, int followers, int following, int posts, DateTime record_time)
        {
            Name = name;
            Followers = followers;
            Following = following;
            Posts = posts;
            RecordTime = record_time;
        }

        public override string ToString()
        {
            return $"Account: {Name}, Followers: {Followers}, Following: {Following}, Posts: {Posts}, Collection Date: {RecordTime}";
        }
    }
}
