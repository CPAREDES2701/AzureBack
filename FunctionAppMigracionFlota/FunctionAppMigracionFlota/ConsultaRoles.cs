using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Nancy.Json;

namespace FunctionAppMigracionFlota
{
    public static class ConsultaRoles
    {
        [FunctionName("ConsultaRoles")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string email = data?.email;

            response resp = new response();
            try
            {
                // SERVICE TOKEN ENDPOINT
                HttpWebRequest serviceToken = (HttpWebRequest)WebRequest.Create("https://login.microsoftonline.com/b7e26f48-2292-4a14-a355-1aeb8489ae3d/oauth2/v2.0/token");
                serviceToken.Headers["host"] = "login.microsoftonline.com";
                serviceToken.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                serviceToken.Method = "POST";

                using (var streamWriter = new StreamWriter(serviceToken.GetRequestStream()))
                {
                    //string json = "grant_type=client_credentials&client_id=84ba9e14-690c-4a35-b08b-c2f365dff658&client_secret=MGx7Q~Sj0~0xDTheDWvzObvBSvPz~OrVdcLIq&scope=https://graph.microsoft.com/.default";
                    string json = "grant_type=client_credentials&client_id=9646306c-045b-432b-99fe-6bc4a8d9c363&client_secret=HRR8Q~ifGn6KGtZ3~YOJ7HdtM5~SfU_t5fyq3cGk&scope=https://graph.microsoft.com/.default";
                    streamWriter.Write(json);
                }
                WebResponse response = serviceToken.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string res = sr.ReadToEnd();
                dynamic list = JsonConvert.DeserializeObject<dynamic>(res);
                string token = list.access_token;

                // SERVICE ROLES
                string URL = "https://graph.microsoft.com/v1.0/users/" + email + "/getMemberGroups";
                HttpWebRequest serviceRoles = (HttpWebRequest)WebRequest.Create(URL);
                serviceRoles.ContentType = "application/json";
                serviceRoles.Headers["Authorization"] = token;
                serviceRoles.Method = "POST";

                using (var streamWriterRol = new StreamWriter(serviceRoles.GetRequestStream()))
                {
                    string Rolesjson = new JavaScriptSerializer().Serialize(new
                    {
                        securityEnabledOnly = true
                    });
                    streamWriterRol.Write(Rolesjson);
                }
                WebResponse responseRoles = serviceRoles.GetResponse();
                StreamReader srRoles = new StreamReader(responseRoles.GetResponseStream(), Encoding.UTF8);

                string resRol = srRoles.ReadToEnd();
                dynamic listRol = JsonConvert.DeserializeObject<dynamic>(resRol);
                string[] tokenRoles = listRol.value.ToObject<string[]>();

                resp.id_Mensaje = "0";
                resp.mensaje = "Se obtuvo correctamente los roles";
                resp.roles = tokenRoles;
                return new OkObjectResult(resp);
            }
            catch (Exception ex)
            {
                resp.id_Mensaje = "1";
                resp.mensaje = "" + ex.Message;
                return new OkObjectResult(resp);
            }
        }

        public class response
        {
            public string mensaje { get; set; }
            public string id_Mensaje { get; set; }
            public string[] roles { get; set; }
        }
    }
}
