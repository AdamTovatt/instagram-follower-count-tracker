using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FollowerCountDatabaseTools.Models
{
    public class AccountInfo
    {
        public int Followers { get; }
        public int Following { get; }
        public int Posts { get; }
        public string Name { get; }

        public AccountInfo(int followers, int following, int posts, string name)
        {
            Followers = followers;
            Following = following;
            Posts = posts;
            Name = name;
        }

        public override string ToString()
        {
            return $"Account: {Name}, Followers: {Followers}, Following: {Following}, Posts: {Posts}";
        }
    }

}
