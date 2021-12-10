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
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FunctionAppMigracionFlota
{
    public static class SubirArchivoAzure
    {
        [FunctionName("SubirArchivoAzure")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string AccountKey = "";
            string AccountName = data?.AccountName;
            string ImageContainer = data?.ImageContainer;
            string imageBase64 = data?.imageBase64;
            string Nombrearchivo = data?.NombreArchivo;

            if (AccountName == "rgeastustasasapbtp")
            {
                AccountKey = "QjD2WpYWhys46KuSikF3GQVmJnyQeWQYy+bplsQbdp6Jl5b5S0BrAA+I3O3CynFlhJg9hAa785fiQzsdBoVh7Q==";
            }
            else
            {
                AccountKey = "tImUXDzGiSNNf2Tkmy0q4fL9Hax01pzhkwoJnZELZ9lNimCDeIUnHSbtZsEHURAxHw0ua1tIFGGQY6EFrs68Fg==";
            }

            AzureStorageConfig ac = new AzureStorageConfig();
            ac.AccountKey = AccountKey;
            ac.AccountName = AccountName;
            ac.ImageContainer = ImageContainer;
            response resp = new response();
            try
            {
                byte[] imageBytes = Convert.FromBase64String(imageBase64);
                MemoryStream ms = new MemoryStream(imageBytes);
                Stream stream = new MemoryStream(imageBytes);

                await UploadFileToStorage(ms, Nombrearchivo, ac);

                resp.id_Mensaje = "0";
                resp.mensaje = "Se subio el archivo correctamente";
                return new OkObjectResult(resp);
            }
            catch (Exception ex)
            {
                resp.id_Mensaje = "1";
                resp.mensaje = "Hubo un error en la subida : " + ex.Message;
                return new OkObjectResult(resp);
            }

        }

        public static Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Upload the file
            //file = new FileStream(fileStream, FileMode.Open);
            blockBlob.UploadFromStreamAsync(fileStream);
            //blockBlob.DownloadToStreamAsync

            return Task.FromResult(true);
        }

        public class AzureStorageConfig
        {
            public string AccountName { get; set; }
            public string AccountKey { get; set; }
            public string ImageContainer { get; set; }
        }

        public class response
        {
            public string mensaje { get; set; }
            public string id_Mensaje { get; set; }
        }
    }
}
