namespace FollowerCountDatabaseTools.Models
{
    public class AccountInfoDataPoint
    {
        public string Name { get; }
        public int Followers { get; }
        public int Following { get; }
        public int Posts { get; }
        public DateTime CollectionDate { get; }

        public AccountInfoDataPoint(string name, int followers, int following, int posts, DateTime collectionDate)
        {
            Name = name;
            Followers = followers;
            Following = following;
            Posts = posts;
            CollectionDate = collectionDate;
        }

        public override string ToString()
        {
            return $"Account: {Name}, Followers: {Followers}, Following: {Following}, Posts: {Posts}, Collection Date: {CollectionDate}";
        }
    }
}
