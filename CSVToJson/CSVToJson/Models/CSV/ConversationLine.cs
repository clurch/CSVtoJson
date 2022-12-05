namespace CSVToJson.Models.CSV
{
	public class ConversationLine
	{
        public string? HashInteractionId { get; set; }
        public string? HashCaseId { get; set; }
        public ContactType ContactType { get; set; }
        public Message Message { get; set; }
    }
}

