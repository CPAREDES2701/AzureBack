using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FunctionAppMigracionFlota
{
    public static class VerArchivo
    {
        [FunctionName("VerArchivo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string vvsystem_code = req.Query["system_code"];

                AzureStorageConfig ac = new AzureStorageConfig();
                ac.AccountKey = "tImUXDzGiSNNf2Tkmy0q4fL9Hax01pzhkwoJnZELZ9lNimCDeIUnHSbtZsEHURAxHw0ua1tIFGGQY6EFrs68Fg=="; //"RtXdm5qPgX+pAV9OohRZYJQ77Rgidgh1vcRP99K86bLGRPy5X4MGl9Lbwz1yGlvodeYMiY/nG0470UW+clRvRg==";
                ac.AccountName = "rgeastustasasapbtpqas"; //"storagepasmutuos";
                ac.ImageContainer = "productos";  //"pass";

                Byte[] resultado = await DownloadFileToStorage(vvsystem_code, ac);

                return new FileContentResult(resultado, "image/jpeg");
            }
            catch (Exception e)
            {
                return new OkObjectResult(e.Message);
            }
        }

        static async Task<byte[]> DownloadFileToStorage(string fileName, AzureStorageConfig _storageConfig)
        {
            dynamic base64 = "";
            resultadoDownload resultado = new resultadoDownload();
            try
            {
                // Cree el objeto storagecredentials leyendo los valores de la configuración (appsettings.json)
                StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

                // Cree una cuenta de almacenamiento en la nube pasando las credenciales de almacenamiento
                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

                // Crea la clienta de blob
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Obtenga una referencia al contenedor de blobs pasando el nombre leyendo el valor de la configuración(appsettings.json)
                CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

                // Obtener la referencia al blob en bloque del contenedor
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

                var Stream = new MemoryStream();

                await blockBlob.DownloadToStreamAsync(Stream);


                byte[] buffer = Stream.ToArray();

                //base64 = Convert.ToBase64String(buffer);

                //resultado.Base64 = base64;
                //resultado.Estado = true;

                return buffer;
            }
            catch (Exception ex)
            {
                resultado.Base64 = ex.Message;
                resultado.Estado = false;
                byte[] buffer = new byte[0];
                return buffer;
            }


            //  return resultado;
        }

        public class AzureStorageConfig
        {

            public string AccountName { get; set; }
            public string AccountKey { get; set; }

            public string ImageContainer { get; set; }
        }

        public class DescargaArchivoResponse
        {
            public string Mensaje { get; set; }
            public bool Estado { get; set; }
            public string Base64 { get; set; }
            public string Extension { get; set; }
        }

        public class resultadoDownload
        {
            public string Base64 { get; set; }
            public bool Estado { get; set; }
        }
    }
}
