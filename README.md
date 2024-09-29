# Instagram Follower Count Tracker

### What is this?
This is a worker service that will fetch some account information about an Instagram account once a day between 3 and 5 in the morning.

The account information that will be fetched includes:
- Follower count (the number of followers the account has),
- Following count (the number of accounts the account follows),
- Post count (the number of posts the account has).

This information will be written to a PGSQL database table along with the name of the account and a timestamp indicating when the account information was fetched.

### Why is this?
The purpose of this service is to run continuously (perhaps on a Raspberry Pi or similar device) to understand how a single or multiple Instagram accounts perform over time.

Specifically, I created this to keep track of all the Instagram pages of the student nations in Lund during my time as Head of PR for Wermlands Nation. I wanted to understand how our account performed over time compared to the other nations.

### How do I use this?

To run the program, build it and then execute it with command line arguments that specify a connection string to a PGSQL database, api-key for sending email updates via postmark, recipent email, sender email and the usernames of the accounts you want to track.

Like this:
```
postgres://username:password@hostname:port/database_name api-key recipient@email.com sender@email.com wermlandsnation gbgnation ostgota vgnationlund smalandsnation lundsnation malmonation helsingkrona sydskanska kristianstadsnation blekingska hallandsnation kalmarnation
```
Notice that the first argument is the connection string as a URL, then the next is a postmark api key, the next is the recipient email address for weekly updates, the next is the sender email for the weekly updates and the following arguments are the names of the Instagram accounts you want to track. You can specify from 1 to as many account names as you want, separated by spaces. The account names can be easily obtained by searching for the account on Instagram in a browser or using Google to search for the Instagram account.

The recipient email and sender email can (and probably will) be the same. Like your own email. The api key can be obtained from postmark. If a string that is less than 10 characters long is provided for the api key argument, the program will consider it an invalid key and not send any emails. If you don't want to use the email feature, just provide a string that is less than 10 characters long for the api key and for the different email addresses.
Like this:
```
postgres://username:password@hostname:port/database_name null null null wermlandsnation gbgnation ostgota vgnationlund smalandsnation lundsnation malmonation helsingkrona sydskanska kristianstadsnation blekingska hallandsnation kalmarnation
```

This will lead you to a page with a URL that looks something like this:
```
https://www.instagram.com/wermlandsnation/
```
In this case, the account name is:
```
wermlandsnation
```

#### Setup
I suggest creating a PostgreSQL database on a Linux-based server and then creating a systemd service that runs the built version of this program. It is designed to be kept running, so the service doesn't need to restart it periodically—just start it once and let it run. Of course, if it crashes, the service can restart it.

## How does it work?
It works by scraping Instagram while pretending to be a postman instance, then extracting the account information from the HTML obtained. It will collect account information once upon startup so that it is easy to verify that it works (it will log a message indicating that it collected information). 

After that, it will only collect account information once a day between 3 and 5 in the morning. It sleeps for an hour at a time during the day and periodically wakes up to check if it is time to collect data. It always writes to the database when it collects data.

Since this is a scraper, it might break in the future if Instagram changes the way they return information in the HTML, or if Instagram tries to block scrapers. Currently, Instagram seems to have a reasonable policy regarding scrapers: large-scale scraping is not allowed, but occasional scraping of a few accounts for personal use is permitted. This falls under the latter category, so it should be fine. It is also polite to Instagram’s servers, as it waits a second between each request, making a few requests a day with a one-second delay, which I hope should be acceptable to Instagram.
