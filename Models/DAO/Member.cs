namespace MyPCSpec.Models.DAO
{
    public class Member
    {
        public int Seq { get; set; }
        public string Id { get; set; }
        public string Pw { get; set; }
        public string Name { get; set; }
        public DateTime birth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string UseYn { get; set; }
        public string DelYn { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }

        public Member()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }
}
