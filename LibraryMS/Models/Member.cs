namespace LibraryMS.Models
{
    /// <summary>Represents a registered library member.</summary>
    public class Member
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public MembershipType MembershipType { get; set; } = MembershipType.Standard;
        public DateTime RegisteredOn { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        /// <summary>Maximum number of books this member can borrow at once.</summary>
        public int BorrowLimit => MembershipType == MembershipType.Premium ? 7 : 3;

        public override string ToString() =>
            $"[{Id}] {Name} | {Email} | {MembershipType} Member | Active: {IsActive}";
    }
}
