using FollowerCountDatabaseTools.Models;
using System.Text.RegularExpressions;

namespace InstagramFollowerCountTracker
{
    public class Instagram
    {
        public static Instagram Instance { get { if (_instance == null) _instance = new Instagram(); return _instance; } }
        private static Instagram? _instance;

        private HttpClient httpClient;

        public Instagram()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.29.2");
        }

        public async Task<AccountInfo?> GetAccountInfoAsync(string username)
        {
            string url = $"https://www.instagram.com/{username}/?hl=en";
            string response = await httpClient.GetStringAsync(url);

            return ExtractAccountInfo(response, username);
        }

        private AccountInfo? ExtractAccountInfo(string html, string username)
        {
            List<int> contentIndexes = FindAllIndexes(html, "content");
            List<string> chunks = ExtractSubstrings(html, contentIndexes, "/>", "followers", "following");

            string? firstChunk = chunks.FirstOrDefault(); // should contain the right information

            if (firstChunk == null) return null;

            return CreateAccountInfoFromAccountInfoText(firstChunk, username);
        }

        private List<string> ExtractSubstrings(string text, List<int> startIndexes, string stoppingString, params string[] hasToContain)
        {
            List<string> substrings = new List<string>();

            foreach (int startIndex in startIndexes)
            {
                // Find the stopping string from the start index
                int stoppingIndex = text.IndexOf(stoppingString, startIndex);

                // If the stopping string is found, extract the substring from start index to stopping index
                if (stoppingIndex != -1)
                {
                    string substring = text.Substring(startIndex, stoppingIndex - startIndex).ToLower();

                    if (hasToContain.All(substring.Contains))
                        substrings.Add(substring);
                }
                else
                {
                    // If the stopping string is not found, extract the remaining part of the text
                    string substring = text.Substring(startIndex).ToLower();

                    if (hasToContain.All(substring.Contains))
                        substrings.Add(substring);
                }
            }

            return substrings;
        }

        // Static method to extract AccountInfo from text
        private static AccountInfo? CreateAccountInfoFromAccountInfoText(string input, string name)
        {
            // Regular expression to match the follower, following, and post counts
            string pattern = @"content=""([\d,]+) followers, ([\d,]+) following, ([\d,]+) posts";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                // Parse the follower count (remove commas)
                int followerCount = int.Parse(match.Groups[1].Value.Replace(",", ""));
                int followingCount = int.Parse(match.Groups[2].Value.Replace(",", ""));
                int postCount = int.Parse(match.Groups[3].Value.Replace(",", ""));

                // Return the new AccountInfo object using the provided name
                return new AccountInfo(followerCount, followingCount, postCount, name);
            }
            else
            {
                return null; // Return null if input format is incorrect
            }
        }

        private List<int> FindAllIndexes(string text, string word)
        {
            List<int> indexes = new List<int>();
            int index = text.IndexOf(word);

            while (index != -1)
            {
                indexes.Add(index);
                index = text.IndexOf(word, index + word.Length);
            }

            return indexes;
        }
    }
}
