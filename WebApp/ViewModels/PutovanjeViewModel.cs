namespace WebApp.ViewModels
{
    public class PutovanjeViewModel
    {
        public int IdPutovanja { get; set; }
        public string Opis { get; set; }
        public int Cijena { get; set; }

        public DateTime DatumPolaska { get; set; }

        public DateTime DatumPovratka { get; set; }
    }
}
