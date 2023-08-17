using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Facturaya.Function
{
    public class T_EnvioBoletas
    {
        [FunctionName("T_EnvioBoletas")]
        public async Task RunAsync([TimerTrigger("0 */2 * * *")]TimerInfo myTimer, ILogger log)
        {
            try
            {
                using var httpClient = new HttpClient();

                // Fetch the list of empresas
                var serviceUrlEmpresas = "https://facturayaapi.azurewebsites.net/api/facturaya/v1/dtes/Empresas";
                var empresaList = new List<EmpresaModel>();

                using (var response = await httpClient.GetAsync(serviceUrlEmpresas))
                {
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    empresaList = System.Text.Json.JsonSerializer.Deserialize<List<EmpresaModel>>(apiResponse, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                }

                foreach (var empresa in empresaList)
                {
                    var rutSii = "60803000-K";
                    var serviceUrl = "https://facturayaapi.azurewebsites.net/api/facturaya/v1/boletas/BoletasGeneradas";
                    var startDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                    var endDate = DateTime.Now.ToString("yyyy-MM-dd");

                    XmlEnvioSii xmlEnvioSii = new XmlEnvioSii();
                    xmlEnvioSii.RutEmpresa = empresa.IdEmpresa;
                    xmlEnvioSii.RutEnvia = empresa.RutRepresentanteLegal;
                    xmlEnvioSii.RutReceptor = rutSii;
                    xmlEnvioSii.NroResolucion = empresa.NroResolucion.ToString();
                    xmlEnvioSii.FechaResolucion = empresa.FechaResolucion.Value.ToString("dd-MM-yyyy");
                    xmlEnvioSii.StorageAccountName = empresa.StorageAccountName;
                    xmlEnvioSii.StorageAccountKey = empresa.StorageAccountKey;
                    xmlEnvioSii.BlobContainerCertificados = empresa.BlobContainerCertificados;
                    xmlEnvioSii.BlobContainerEmitidos = empresa.BlobContainerEmitidos;
                    xmlEnvioSii.BlobsNamesDtes = new List<string>();

                    using (var response = await httpClient.GetAsync($"{serviceUrl}/{empresa.IdEmpresa}/{startDate}/{endDate}"))
                    {
                        var apiResponse = await response.Content.ReadAsStringAsync();
                        var boletasGeneradasList = System.Text.Json.JsonSerializer.Deserialize<List<BoletasGeneradasModel>>(apiResponse);

                        Console.WriteLine($"Cantidad de boletas totales: {boletasGeneradasList.Count}");

                        foreach (var boletaGenerada in boletasGeneradasList)
                        {
                            xmlEnvioSii.BlobsNamesDtes.Add(boletaGenerada.BlobNameDTE);
                        }
                    }

                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(xmlEnvioSii);
                    string connectionString = "DefaultEndpointsProtocol=https;AccountName=facturayaprd;AccountKey=wH8jV6UxBDLrKI3WsQm3esYm/2JlxqWylJ9QpottgPNpH7xmCCHcx/BhP/+QRkpvffsUwRQEb3nxP28JmpsFtw==";
                    var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
                    var cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
                    var cloudQueue = cloudQueueClient.GetQueueReference("generaboletasetv2");
                    var cloudQueueMessage = new CloudQueueMessage(json);
                    await cloudQueue.AddMessageAsync(cloudQueueMessage);

                    // Wait for 10 seconds before enqueuing the next message
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
