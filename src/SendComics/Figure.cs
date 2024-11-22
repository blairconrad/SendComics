namespace SendComics;

public record Figure(string ImageLocation)
{
    public string Caption { get; init; }

    public bool HasCaption => !string.IsNullOrWhiteSpace(this.Caption);
}
