#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log, 
    CloudTable eventsTable, string id, string responseCode)
{
    log.Info($"Updating response {responseCode} for event {id} ");
    TableOperation operation = TableOperation.Retrieve<EventTableEntity>("event", id);
    TableResult result = eventsTable.Execute(operation);
    var @event = (EventTableEntity)result.Result;
    if (@event == null)
    {
        log.Warning($"failed to find event {id}");
        return req.CreateResponse(HttpStatusCode.NotFound, "Invalid event");
    }

    log.Info("deserializing");
    var responses = JsonConvert.DeserializeObject<Response[]>(@event.ResponsesJson);
    var responseToUpdate = responses.FirstOrDefault(r => r.ResponseCode == responseCode);
    if (responseToUpdate == null)
    {
        log.Warning($"failed to find response {responseCode}");
        return req.CreateResponse(HttpStatusCode.NotFound, "Invalid event");
    }
    log.Info("getting body");
    dynamic response = await req.Content.ReadAsAsync<object>();
    responseToUpdate.IsPlaying = response.isPlaying;
    @event.ResponsesJson = JsonConvert.SerializeObject(responses);

    operation = TableOperation.Replace(@event);
    eventsTable.Execute(operation);

    return req.CreateResponse(HttpStatusCode.OK, "Updated successfully");
}

public class EventTableEntity : TableEntity 
{
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