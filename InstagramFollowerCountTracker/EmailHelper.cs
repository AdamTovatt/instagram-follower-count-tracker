using PostmarkDotNet;

namespace InstagramFollowerCountTracker
{
    public class EmailHelper
    {
        private string apiKey;
        private string reportRecipient;
        private string reportSender;

        public EmailHelper(string apiKey, string reportRecipient, string reportSender)
        {
            this.apiKey = apiKey;
            this.reportRecipient = reportRecipient;
            this.reportSender = reportSender;
        }

        public async Task SendGraphReport(byte[] totalGraph, byte[] singleGraph)
        {
            PostmarkMessage message = new PostmarkMessage()
            {
                To = reportRecipient,
                From = reportSender,
                TrackOpens = false,
                Subject = $"Instagram followers report {DateTime.Now.ToString("yyyy-MM-dd")}",
                TextBody = "Here is your weekly instagram followers report.",
                HtmlBody = "Here is your weekly instagram followers report",
                Tag = "followers-report",
            };

            message.AddAttachment(totalGraph, "total.jpeg", "image/jpg");
            message.AddAttachment(singleGraph, "single.jpeg", "image/jpg");

            PostmarkClient client = new PostmarkClient(apiKey);
            await client.SendMessageAsync(message);
        }
    }
}
