#r "Newtonsoft.Json"

using System.Net;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, EventTableEntity @event, string id, TraceWriter log)
{
    log.Info($"GetEvent function was called for {id}.");
    if (@event == null)    
    {
        log.Warning($"failed to find event {id}");
        return req.CreateResponse(HttpStatusCode.NotFound, "Invalid event");
    } 
    var responses = JsonConvert.DeserializeObject<Response[]>(@event.ResponsesJson);
    // parse query parameter
    string responseCode = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "responseCode", true) == 0)
        .Value;

    var myResponse = responses.FirstOrDefault(r => r.ResponseCode == responseCode); 
    if (myResponse != null)
    {
        return req.CreateResponse(HttpStatusCode.OK, new {
            EventDateAndTime = @event.EventDateAndTime,
            Location = @event.Location,
            MyResponse = myResponse.IsPlaying,
            Responses = responses.Select(r => new {
                Name = r.Name, Playing = r.IsPlaying }).ToArray()
        } );
    }
    return req.CreateResponse(HttpStatusCode.NotFound, "Invalid response code");


}

public class EventTableEntity 
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTime EventDateAndTime { get; set; }
    public string Location { get; set; }
    public string ResponsesJson { get; set; }
}

public class Response 
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string IsPlaying { get; set; }
    public string ResponseCode { get; set; }
}
