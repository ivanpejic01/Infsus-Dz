namespace WebApp.ViewModels;

    public class PutovanjaViewModel
    {
    public IEnumerable<PutovanjeViewModel> putovanja { get; set; }
    public int CurrentPageNumber { get; set; } // Trenutni broj stranice
    public int TotalPages { get; set; } // Ukupan broj stranica
    public int PageSize { get; set; } // Broj stavki po stranici
    public int TotalCount { get; set; } // Ukupan broj stavki

    public int PageNumber { get; set; }
}

