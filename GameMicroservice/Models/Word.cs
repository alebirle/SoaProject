namespace GameMicroservice.Models;

public class Word
{
    public Guid Id { get; set; }
    public string ChosenWord { get; set; }
    public DateTime Date { get; set; }
}
