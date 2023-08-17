using System;

public class EmpresaModel
{
    public string IdEmpresa { get; set; }
    public string RazonSocial { get; set; }
    public string Direccion { get; set; }
    public Nullable<int> CodigoPostal { get; set; }
    public string Comuna { get; set; }
    public string Ciudad { get; set; }
    public string Telefono { get; set; }
    public string Contacto { get; set; }
    public string MailContacto { get; set; }
    public string Giro { get; set; }
    public string Acteco { get; set; }
    public Nullable<int> NroResolucion { get; set; }
    public Nullable<System.DateTime> FechaResolucion { get; set; }
    public string DireccionRegional { get; set; }
    public string RutRepresentanteLegal { get; set; }
    public string StorageAccountName { get; set; }
    public string StorageAccountKey { get; set; }
    public string BlobContainerCertificados { get; set; }
    public string BlobContainerEmitidos { get; set; }
}