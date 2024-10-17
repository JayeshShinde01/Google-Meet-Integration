using GoogleAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Web.Mvc;

namespace GoogleAPI.Controllers
{
    public class CalendarEventController : Controller
    {
        [HttpPost]
        public ActionResult CreateEvent(Event calendarEvent)
        {

           
            var tokenFile = "C:\\Users\\DELL\\source\\repos\\google-calendar-integration\\Google API\\Files\\tokens.json";
            var tokens = JObject.Parse(System.IO.File.ReadAllText(tokenFile));

           
            RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/calendars/primary/events");

           
            RestRequest request = new RestRequest(Method.POST);

            
            calendarEvent.Start.DateTime = DateTime.Parse(calendarEvent.Start.DateTime).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
            calendarEvent.End.DateTime = DateTime.Parse(calendarEvent.End.DateTime).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

            calendarEvent.ConferenceData = new ConferenceData()
            {
                CreateRequest = new CreateConferenceRequest()
                {
                    RequestId = Guid.NewGuid().ToString(),
                    ConferenceSolutionKey = new ConferenceSolutionKey()
                    {
                        Type = "hangoutsMeet"  
                    }
                }
            };

            
            var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            
            request.AddQueryParameter("conferenceDataVersion", "1"); 
            request.AddHeader("Authorization", "Bearer " + tokens["access_token"]); 
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");

           
            request.AddParameter("application/json", model, ParameterType.RequestBody);

            
            var response = restClient.Execute(request);

         
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var createdEvent = JObject.Parse(response.Content);

                // Extract the Google Meet link from the response
                var googleMeetLink = createdEvent["conferenceData"]?["entryPoints"]?[0]?["uri"]?.ToString();

                if (!string.IsNullOrEmpty(googleMeetLink))
                {
                    // If Google Meet link was created successfully, return success
                    ViewBag.MeetLink = googleMeetLink;
                    return View("ShowMeetLink");
                }
                else
                {
                    // If no Meet link was created, return error
                    return View("Error", model: "Failed to generate Google Meet link.");
                }
            }
            else
            {
                // If API request failed, return error with response content
                var errorDetails = response.Content;
                return View("Error", model: errorDetails);
            }
        }
    }
}
