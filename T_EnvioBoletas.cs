using System;
using System.Collections.Generic;
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
                log.LogInformation("Iniciando proceso de envío de boletas");
                using var httpClient = new HttpClient();

                log.LogInformation("Obteniendo datos de empresas");
                var serviceUrlEmpresas = Environment.GetEnvironmentVariable("ServiceUrlEmpresas");
                var empresaList = new List<EmpresaModel>();

                using (var response = await httpClient.GetAsync(serviceUrlEmpresas))
                {
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    empresaList = System.Text.Json.JsonSerializer.Deserialize<List<EmpresaModel>>(apiResponse, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                }

                log.LogInformation("Recorriendo listado de empresas");
                foreach (var empresa in empresaList)
                {
                    log.LogInformation($"RUT: { empresa.IdEmpresa } razón social: { empresa.RazonSocial } ");
                    var rutSii = Environment.GetEnvironmentVariable("RutSii");
                    var serviceUrl = Environment.GetEnvironmentVariable("ServiceUrlBoletas");
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

                        log.LogInformation($"Cantidad de boletas totales: {boletasGeneradasList.Count}");

                        foreach (var boletaGenerada in boletasGeneradasList)
                        {
                            xmlEnvioSii.BlobsNamesDtes.Add(boletaGenerada.BlobNameDTE);
                        }
                    }

                    log.LogInformation("Encolando mensaje para procesamiento de boletas");
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(xmlEnvioSii);
                    string connectionString = Environment.GetEnvironmentVariable("QueueConnectionString");
                    var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
                    var cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
                    var cloudQueue = cloudQueueClient.GetQueueReference(Environment.GetEnvironmentVariable("QueueName"));
                    var cloudQueueMessage = new CloudQueueMessage(json);
                    await cloudQueue.AddMessageAsync(cloudQueueMessage);
                    log.LogInformation("Mensaje de procesamiento encolado");

                    // Wait for 10 seconds before enqueuing the next message
                    log.LogInformation("Esperando 10 segundos para siguiente iteración");
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error: {ex.Message}");
            }
            finally
            {
                log.LogInformation("Fin de proceso de envío de boletas");
            }
        }
    }
}
