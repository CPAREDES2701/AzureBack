using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;

namespace FunctionAppMigracionFlota
{
    public static class LogEventoDescarga
    {
        [FunctionName("LogEventoDescarga")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            respuestaServ data1 = new respuestaServ();
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string App = data?.App;
            string FechaReg = data?.FechaReg;
            string CantDesc = data?.CantDesc;
            string Usuario = data?.Usuario;
            string NroMarea = data?.NroMarea;


            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection cnn = new SqlConnection(str))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cnn;
                //SqlDataAdapter sqlDA;

                cmd.CommandText = "[Fishing].[sp_GrabarLogDescargaXMarea]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@App", App);
                cmd.Parameters.AddWithValue("@FechaReg", FechaReg);
                cmd.Parameters.AddWithValue("@CantDesc", CantDesc);
                cmd.Parameters.AddWithValue("@Usuario", Usuario);
                cmd.Parameters.AddWithValue("@NroMarea", NroMarea);

                try
                {
                    cnn.Open();
                    cmd.ExecuteNonQuery();

                    data1.mensaje = "Se guardo el registro correctamente";
                    data1.id_mensaje = "0";
                }
                catch (Exception ex)
                {
                    cnn.Close();
                    // throw the exception  
                    string error = ex.Message;
                    data1.mensaje = error;
                    data1.id_mensaje = "1";
                }
                finally
                {
                    cnn.Close();
                }


            }

            return new OkObjectResult(data1);
        }

        public class respuestaServ
        {
            public string mensaje { get; set; }
            public string id_mensaje { get; set; }

        }
    }
}
