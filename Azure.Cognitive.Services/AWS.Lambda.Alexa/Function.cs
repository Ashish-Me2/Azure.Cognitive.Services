using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWS.Lambda.Alexa
{
    public class LunchMenuHandler
    {
        ILambdaLogger log = null;
        public List<FactResource> GetResources()
        {
            List<FactResource> resources = new List<FactResource>();
            FactResource enINResource = new FactResource("en-IN");
            enINResource.SkillName = "Chirec School Lunch Menu";
            enINResource.GetFactMessage = "Here's your requested menu: ";
            enINResource.HelpMessage = "You can say what's in lunch for Monday, or, you can say exit... What can I help you with?";
            enINResource.HelpReprompt = "You can say tell me the lunch menu for Monday to get started";
            //enINResource.StopMessage = "Enjoy your lunch...";
            enINResource.StopMessage = String.Empty;
            //enINResource.Facts.Add("Uff. Kyaa keh rahe ho yaar...");
            resources.Add(enINResource);
            return resources;
        }

        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse MenuHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;
            log = context.Logger;
            log.LogLine($"Skill Request Object:");
            log.LogLine(JsonConvert.SerializeObject(input));

            var allResources = GetResources();
            var resource = allResources.FirstOrDefault();

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, ask School Lunch Menu");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = emitNewFact(resource, true);

            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;
                log.LogLine("----------------------------------------------------------");
                log.LogLine("Intent Resolver: " + intentRequest.Intent.Name + ", " + intentRequest.Intent.Slots["weekday"].Value);
                log.LogLine("----------------------------------------------------------");

                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
                        break;
                    case "GetMenu":
                        log.LogLine($"GetFactIntent sent: Get Menu with slot value:" + (intentRequest.Intent.Slots["weekday"].Value));
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetMenuItems(intentRequest.Intent.Slots["weekday"].Value).Result;
                        response.Response.ShouldEndSession = true;
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                }
            }

            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            log.LogLine($"Skill Response Object...");
            log.LogLine(JsonConvert.SerializeObject(response));
            return response;
        }

        public async Task<string> GetMenuItems(string weekDay)
        {
            ExtServiceHelper service = new ExtServiceHelper();
            string _weekDay = String.Empty;
            List<string> weekDayNames = new List<string> { "SUNDAY", "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY" };
            if ((String.IsNullOrEmpty(weekDay))||(!weekDayNames.Contains(weekDay.ToUpper())))
            {
                weekDay = null;
            }
            if (String.IsNullOrEmpty(weekDay))
            {
                DateTime today = DateTime.Now;
                _weekDay = today.AddDays(1).DayOfWeek.ToString().ToUpper();
                if ((_weekDay.Equals("SATURDAY") || (_weekDay.Equals("SUNDAY"))))
                {
                    _weekDay = "MONDAY";
                }
            }
            else
            {
                _weekDay = weekDay;
            }
            string menu = "The menu for " + _weekDay + " is " + await service.GetDataFromService("https://ocrserviceapi.azurewebsites.net/", "api/ocr?Weekday=", new List<object> { _weekDay });
            log.LogLine($"Ext API Response: " + menu);
            return menu;
        }

        public string emitNewFact(FactResource resource, bool withPreface)
        {
            Random r = new Random();
            if (withPreface)
                return resource.GetFactMessage + resource.Facts[r.Next(resource.Facts.Count)];
            return resource.Facts[r.Next(resource.Facts.Count)];
        }
    }
}