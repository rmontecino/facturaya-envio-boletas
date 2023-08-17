using System.Collections.Generic;

public class XmlEnvioSii
{
    public string RutEmpresa { get; set; }
    public string RutEnvia { get; set; }
    public string RutReceptor { get; set; }
    public string FechaResolucion { get; set; }
    public string NroResolucion { get; set; }
    public List<string> BlobsNamesDtes { get; set; }
    public string StorageAccountName { get; set; }
    public string StorageAccountKey { get; set; }
    public string BlobContainerEmitidos { get; set; }
    public string BlobContainerCertificados { get; set; }
}