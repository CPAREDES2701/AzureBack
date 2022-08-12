using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Storage.Blobs;
using System.Collections.Generic;

namespace FunctionAppMigracionFlota
{
    public static class ConsultaArchivos
    {
        [FunctionName("ConsultaArchivos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string AccountKey = "";
            string AccountName = data?.AccountName;
            string ImageContainer = data?.ImageContainer;
            string Prefix = data?.Prefix;

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
                StorageCredentials storageCredentials = new StorageCredentials(ac.AccountName, ac.AccountKey);
                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(ac.ImageContainer);
                await UploadFileToStorage(Prefix, container);

                resp.id_Mensaje = "0";
                resp.mensaje = "Consulta exitosa";
                resp.ListaArchivos = await UploadFileToStorage(Prefix, container);
                return new OkObjectResult(resp);
            }
            catch (Exception ex)
            {
                resp.id_Mensaje = "1";
                resp.mensaje = "Hubo un error en la consulta : " + ex.Message;
                return new OkObjectResult(resp);
            }

        }

        public static async  Task<List<string>> UploadFileToStorage( string prefix, CloudBlobContainer container)
        {
            List<string> nombresArch = new List<string>();
            CloudBlobDirectory dir;
            CloudBlob blob;
            BlobContinuationToken continuationToken = null;

            try
            {
                do
                {
                    BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(prefix,
                        false, BlobListingDetails.Metadata, null, continuationToken, null, null);
                    int contador = 0;
                    foreach (var blobItem in resultSegment.Results)
                    {
                        // A hierarchical listing may return both virtual directories and blobs.
                        if (blobItem is CloudBlobDirectory)
                        {
                            dir = (CloudBlobDirectory)blobItem;

                            // Write out the prefix of the virtual directory.
                            Console.WriteLine("Virtual directory prefix: {0}", dir.Prefix);
                            await UploadFileToStorage(dir.Prefix,container);
                        }
                        else
                        {
                            // Write out the name of the blob.
                            blob = (CloudBlob)blobItem;
                            nombresArch.Add(blob.Name);
                            Console.WriteLine("Blob name: {0}", blob.Name);
                        }
                        contador++;
                        Console.WriteLine();
                    }

                    // Get the continuation token and loop until it is null.
                    continuationToken = resultSegment.ContinuationToken;

                } while (continuationToken != null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }


            return await Task.FromResult(nombresArch);
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
            public List<string> ListaArchivos { get; set; }
        }
    }
}
