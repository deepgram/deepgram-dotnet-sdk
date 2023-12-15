# TODO -- remove before general release

## Deepgram
- Live client change event args to callback
- <del>Live client - when receiving data type of data needs to be check currently the
    recieve method only expects to recieve LiveTranscriptionResponse but the method 
    can recieve either LiveTranscriptionResponse or LiveMetadataResponse

    

## DeepgramTests
- Test for live client
- <del>Test for Manage client
- <del>Test for OnPrem client
- <del>Test for Prerecorded client

AbstractClient tests--

- Intergration Test need to be written
- Either Intergration soultion needs to be exclude from git action tests   
  or   
  Live key added and hidden from the repo  
  Not sure which is the best approach

- <del>Test to catch Exception, HttpRequestExceptions already being caught
- <del>Tests for the post method that handle streams
- <del>test for the get QueryParametesUtil class to catch exceptions
- <del>ServiceCollectionExtension test
- <del>ClientWebSocketExtensions test - now way yo get the headers or mock clientwebsocket


# Changes
added redundencies in for people not wanting to/ or dont have access to servicecollection(in pre exisiting apps upgrading to this version of the sdk) use Dependency Injection such as if they just want to create a simple
console app demo and new up a client 

added checks to see if there is a predefined client passed in - if there is then the sdk will assume the base address
is in the correct format - the reason being that if it is a onPrem there is no way to knwo what there server address is