namespace HomeWork_65.Models;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime Date { get; set; }
    public int UserId { get; set; }
    public MyUser? User { get; set; }
}