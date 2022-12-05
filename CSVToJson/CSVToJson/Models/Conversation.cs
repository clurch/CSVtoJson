using System;
namespace CSVToJson.Models
{
	public class Conversation
	{
		public string? SessionId { get; set; }
		public string? MasterCallId { get; set; }
		public DateTime? UtcStart { get; set; }
		public DateTime? UtcEnd { get; set; }
		public List<Contact> Contacts { get; set; } = new();
        public object? MetaData { get; set; }
	}
}

