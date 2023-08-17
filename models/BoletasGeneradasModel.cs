using System;

public class BoletasGeneradasModel
{
    public int Id { get; set; }
    public string RUTEmisor { get; set; }
    public string RutReceptor { get; set; }
    public string RznSocRecep { get; set; }
    public int TpoDTE { get; set; }
    public string TipoDTE { get; set; }
    public int NroDTE { get; set; }
    public DateTime FchEmis { get; set; }
    public decimal MntTotal { get; set; }
    public string BlobNameDTE { get; set; }
    public string EstadoDTE { get; set; }
    public bool Seleccionar { get; set; }
}