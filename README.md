# Minimal Api & Redis Url Filter Example

###### Note: The app is not a product! Direct purchase and use may not provide the desired performance. Developed for entertainment purposes only.
---

###### Description:  A sweet project that blocks malicious urls for redis application with minimal api that comes with .Net 6. There is a Swagger plugin in the application. You can access the necessary parameters for the post methods via Swagger.
---

## Features

- Querying the entered url on Redis
- Adding the entered url to Redis
- Deleting the entered url on Redis
- Updating the entered url on Redis
- Listing all urls on Redis

##  Methods

| Method | Endpoint | Request Type |
| ------ | ------ | ------ |
| /UrlCheck | ../UrlCheck?queryUrlFilter="url" | GET
| /AddNewUrl | ../AddNewUrl | POST
| /DeleteUrl | ../DeleteUrl?queryUrlFilter="url" | POST
| /UpdateUrl | ../UpdateUrl | PUT
| /ListAllBlockedUrls | ../ListAllBlockedUrls | GET

## Http Status Codes

###### As a result of the working of the methods, http status codes are returned for the url sent or other operations performed.

### These;


| Method | Status Code | Explanation |
| ------ | ------ | ------ |
| UrlCheck |202 | The url sent is not in the list of malicious urls. (Allowed)
| UrlCheck | 406 | The sent url is in the list of malicious urls. (Permission denied)
| AddNewUrl | 201 | Added new malicious url with sent parameters. (Added)
| DeleteUrl | 200 | The url on Redis was deleted with the sent parameters. (Deleted)
| DeleteUrl | 404 | The url could not be found on Redis with the parameters sent. (Not found)

---
