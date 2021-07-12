# github-scraper
asp.net core playground

##AspGithubClassScraper
Console app that traverses files from remote repository and extracts c# terms. Saves either to MongoDB or PostgreSQL. Will be possible to use both in near future..
Configure repository at appsettings.json. 

##ScraperApi
Use API client to search data. Currently only `name` filter has support. Default pages size is 5. Navigation links return at the LINK header.
Configure Basic Auth credentials via appsettings.json (see appsettings_example.json).
Request example:
```
curl --location --request GET 'http://scraper.localhost/scraper-api/api/entity/files?page=3&name=animEditor' \
--header 'Authorization: Basic cGF2ZWw6UVdFcXdlMTIz'
```

```
curl --location --request GET 'http://scraper.localhost/scraper-api/api/entity/terms?name=test&page=10' \
--header 'Authorization: Basic cGF2ZWw6UVdFcXdlMTIz'
```