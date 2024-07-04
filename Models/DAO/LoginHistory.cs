namespace MyPCSpec.Models.DAO
{
    public class LoginHistory
    {
        public int Seq { get; set; }
        public string Id { get; set; }
        public char FailYn { get; set; }
        public string Ip { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
