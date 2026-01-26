namespace RealLifeLawAssist.Models
{
    public class ContentPart
    {
        public string Text { get; set; }
    }

    public class Content
    {
        public ContentPart[] Parts { get; set; }
    }

    public class GenerateContentRequest
    {
        public Content[] Contents { get; set; }
    }
}
