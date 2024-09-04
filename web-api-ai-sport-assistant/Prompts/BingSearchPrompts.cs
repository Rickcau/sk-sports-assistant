namespace web_api_ai_sport_assistant.NLPSqlPrompts
{
    public static class BingSearchPrompts
    {
        public static string GetBingActionPrompt(string prompt) =>
        $$$"""
        query:{{{prompt}}}

        Instructions:
        Using the provided query return the JSON structure below populated with the topic and actions that user is asking you to perform.  Only output JSON!

        [JSON]
        {
           'topic': '<topic>',
           'data' : [ {'action': '<action1>'},
                      {'action': '<action2>'},
                      {'action': '<action3>'}
            ]
        }
        [JSON end]

        [Examples for JSON Output]
        {
           'topic': 'Georgia vs. North Carolina',
           'data' : [ {'action': 'number of penalties'},
                      {'action': 'number of reviews'},
                      {'action': 'summary of the game'}
            ]
        }
        Extract from the query the topic and actions and populate the JSON with those details. 

        """;
       
       // Not used, just here in case I need a new prompt.
       public static string GetPlaceholderPrompt = @"
       You are responsible for checking the chat history to determine if the request can be answer from the chat history.
       If the current prompt is unrelated to details found in the chat history you must respond in fridendly mannaer to the best of your
       ability based on the context of the user prompt, while also adding the keyword 'Unrelated'.
       
       Examples:
       1. Unrelated Request:
          - User Prompt: Hello, how can you help me?
          - Reponse: 'Unrelated: I can help you get details about the game data stored in our system or details about game details that are available through a Bing Search.'
       
       2. Unrelated Request:
          - User Prompt: 'Thank you'
          - Reponse: 'Unrelated: You'er welcome!'
       ";
    }
}