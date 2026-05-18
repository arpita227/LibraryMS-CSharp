namespace LibraryMS.Models
{
    /// <summary>Tracks a single book borrowing transaction.</summary>
    public class BorrowRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
        public string BookId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;

        // Denormalized for easy display
        public string BookTitle { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;

        public DateTime BorrowedOn { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14); // 2-week default
        public DateTime? ReturnedOn { get; set; } = null;

        public bool IsReturned => ReturnedOn.HasValue;

        /// <summary>Calculates fine for overdue books at ₹5/day.</summary>
        public decimal OverdueFine
        {
            get
            {
                if (IsReturned) return 0m;
                var overdueDays = (DateTime.Now - DueDate).Days;
                return overdueDays > 0 ? overdueDays * 5m : 0m;
            }
        }

        public bool IsOverdue => !IsReturned && DateTime.Now > DueDate;

        public override string ToString() =>
            $"[{Id}] \"{BookTitle}\" → {MemberName} | Due: {DueDate:dd-MMM-yyyy}" +
            (IsReturned ? $" | Returned: {ReturnedOn:dd-MMM-yyyy}" : IsOverdue ? " ⚠ OVERDUE" : "");
    }
}
