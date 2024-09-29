# sk-sports-assistant
This solution leveages an NL2SQL library to allow our GenAI solution to perform complex queries against SQL and to integrate that data into a Chat conversation using AI.  The solution is sports specific and it also leverages the Bing Search API to allow the user to extend the search for sports related stats leveraging the Internet.

# api-ai-sport-assistant
This is a Azure Function implementation of the REST API which provides the GenAI Chat experience.  It leverages two plugins 1) DBQueryPlugin and 2) BingSearchPlugin.

# web-api-ai-sport-assistant
This is a tradition ASP.NET MVC Controller REST API that implements the GenAI Chat experience.  It leverages two plugins 1) DBQueryPlugin and 2) BingSearchPlugin.

# Important Notes
If you have a need to perform aggregrate style queries or need to integrate results of Bing Searches into your GenAI solution, this repo will get you moving in the right direction.  The Bing Plugin also has the ability to scrape the actual page which will give you more details than what you get with the snippet property of the Bing Search Results.
