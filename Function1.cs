using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

namespace testfunctionprh
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];
            string biff = req.Query["biff"];
            string url = req.Query["url"];
            string getflightInfo = req.Query["grabAircraftFlightInfo"];
            string getJackAir = req.Query["JackAir"];
            string trackTail = req.Query["TailNumber"];
            string jhaInsiderTrading = req.Query["JHAInsiderTrading"];

            string responseMessage = "";

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            if (!string.IsNullOrEmpty(name))
            {
                 responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
            }

            if (!string.IsNullOrEmpty(biff))
            {
                responseMessage = string.IsNullOrEmpty(biff)
               ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
               : $"Hello, {biff}. This HTTP triggered function executed successfully.";
            }

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        using (HttpResponseMessage response = client.GetAsync(url).Result)
                        {
                            using (HttpContent content = response.Content)
                            {
                                responseMessage = content.ReadAsStringAsync().Result;
                            }
                        }
                    }
                }
                catch 
                {
                    responseMessage = "OOPS: got an error";
                }
            }

            if (!string.IsNullOrEmpty(getflightInfo))
            {
                try
                {
                    
                    responseMessage = grabAircraftFlightInfo();

                }
                catch
                {
                    responseMessage = "OOPS: got an error";
                }
            }

            if (!string.IsNullOrEmpty(getJackAir))
            {
                try
                {

                    responseMessage = grabJackAirInfo();

                }
                catch
                {
                    responseMessage = "OOPS: got an error";
                }
            }

            if (!string.IsNullOrEmpty(trackTail))
            {
                try
                {

                    responseMessage = trackTheTail(trackTail);

                }
                catch
                {
                    responseMessage = "OOPS: got an error";
                }
            }

            if (!string.IsNullOrEmpty(jhaInsiderTrading))
            {
                try
                {

                    responseMessage = JHAInsider(jhaInsiderTrading);

                }
                catch
                {
                    responseMessage = "OOPS: got an error";
                }
            }

            return new OkObjectResult(responseMessage);
        }

        public static string grabAircraftInfo()
        {
            return null;
        }

        public static string grabAircraftFlightInfo()  // https://stackoverflow.com/questions/27108264/how-to-properly-make-a-http-web-get-request
        {
            string accessKey = "cf9993bc35488eb63cfaf72e41f63cb0";

            var url = "http://api.aviationstack.com/v1/flights?access_key=" + accessKey;
            
            var web = new WebClient();

            var responseString = web.DownloadString(url);

            return responseString;
        }

        public static string grabJackAirInfo()  // https://stackoverflow.com/questions/27108264/how-to-properly-make-a-http-web-get-request
        {

            string responseMeta = "";

            List<String> urls = new List<string>{"https://flightaware.com/live/flight/N894JH",
                        "https://flightaware.com/live/flight/N895JH",
                        "https://flightaware.com/live/flight/N896JH",
                        "https://flightaware.com/live/flight/N897JH",
                         "https://flightaware.com/live/flight/N191JH"};

            int countdown = 1;

            foreach (string jhaurl in urls)
            {

                var responseString = new WebClient().DownloadString(jhaurl);

                var lines = responseString.Split(">");

                foreach (string findmeta in lines)
                {
                    // catch meta name, map, and Track
                    if (findmeta.Contains("meta name") && countdown > 7)
                    {
                        if (findmeta.Contains("Track "))
                        {
                            responseMeta += findmeta.Replace("\" /","").Replace("<meta name=\"twitter:description\" content=\"Track", "");
                        }
                     }
                    else
                    {
                        countdown += 1;
                    }
                }
            }

            return responseMeta;

        }

        public static string trackTheTail(string passedTailNumber)  // https://stackoverflow.com/questions/27108264/how-to-properly-make-a-http-web-get-request
        {
            string responseMeta = "";

            int countdown = 1;

            var responseString = new WebClient().DownloadString("https://flightaware.com/live/flight/" + passedTailNumber);

            var lines = responseString.Split(">");

            foreach (string findmeta in lines)
            {
                // catch meta name, map, and Track
                if (findmeta.Contains("meta name") && countdown > 7)
                {
                    if (findmeta.Contains("Track "))
                    {
                        responseMeta += findmeta.Replace("\" /", "").Replace("<meta name=\"twitter:description\" content=\"Track", "");
                    }
                }
                else
                {
                    countdown += 1;
                }
            }

            return responseMeta;

        }

        // place code to grab sec info for jha here...

        public static string JHAInsider(string CIK)  // https://stackoverflow.com/questions/27108264/how-to-properly-make-a-http-web-get-request
        {
            var url = "https://www.sec.gov/cgi-bin/own-disp?action=getissuer&CIK=" + "0000779152" + "&type=&dateb=&owner=include&start=0"; //.Replace("REPLACEME", CIK);

            //var web = new WebClient();

            string responseMeta = "";

            var responseString = new WebClient().DownloadString(url);

            //var responseString = web.DownloadString(url);

            var lines = responseString.Split(">");

            bool trip = false;

            foreach (string findmeta in lines)
            {
                // catch meta name, map, and Track
                if (findmeta.Contains("your convenience."))
                {
                    trip = true;
                }

                if (trip && findmeta.Contains("</td") && !findmeta.Contains("&nbsp;"))
                {
                    responseMeta += "<td>" + findmeta.Replace("</td", "</td>") + Environment.NewLine;
                }
            }

            return responseMeta;

        }

        public static string makeHTML()
        {
            return null;
        }

        public static string replaceStuff( string[] beGone, string[] replaceWith)
        {
            return null;
        }

        public static string makePretty()
        {
            return null;
        }

        public static string grabURLSource()
        {
            return null;
        }

        public static string getBetweenTwoStrings(string[] findIn, string beginHere, string endHere)
        {
            bool latch = false;
            string responseMeta = "";

            foreach (string findmeta in findIn)
            {
                if (findmeta.Contains(beginHere))
                {
                    latch = true;
                }

                if (!findmeta.Contains(endHere) && latch)
                {
                    responseMeta += findmeta;
                }
                else
                {
                    latch = false;
                }
            }
            return responseMeta;
        }
    }    
}
    