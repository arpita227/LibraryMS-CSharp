namespace LibraryMS.Models
{
    /// <summary>Represents a book in the library catalog.</summary>
    public class Book
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public int TotalCopies { get; set; } = 1;
        public int AvailableCopies { get; set; } = 1;
        public BookStatus Status { get; set; } = BookStatus.Available;
        public DateTime AddedOn { get; set; } = DateTime.Now;

        // Computed property — not stored in JSON
        public bool IsAvailable => AvailableCopies > 0 && Status == BookStatus.Available;

        public override string ToString() =>
            $"[{Id}] \"{Title}\" by {Author} ({PublishedYear}) — {Genre} | Copies: {AvailableCopies}/{TotalCopies}";
    }
}
