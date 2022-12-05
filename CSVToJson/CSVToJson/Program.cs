
using System;
using System.Data;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using CSVToJson.Models;
using CSVToJson.Models.CSV;
using GenericParsing;

class Program
{
    static void Main(string[] args)
    {
        var csvLocation = args[0];
        var jsonDestination = args[1];
        var jsonFileName = jsonDestination.Split('/').LastOrDefault()?.Split('.').FirstOrDefault();

        var dataTable = GetDataTable(csvLocation);
        var conversationLines = GetConversationLines(dataTable);
        var convsersation = GetConverstation(conversationLines, jsonFileName);
        WriteConversationToJsonFile(convsersation, jsonDestination);
    }

    private static DataTable GetDataTable(string csvLocation)
    {
        var adapter = new GenericParserAdapter(csvLocation, Encoding.UTF8);
        return adapter.GetDataTable();
    }

    private static List<ConversationLine> GetConversationLines(DataTable dataTable)
    {
        var conversationLines = new List<ConversationLine>();
        var firstRow = true;

        foreach (DataRow row in dataTable.Rows)
        {
            if (!firstRow)
                conversationLines.Add(GetConversationLine(row));
            else
                firstRow = false;
        }

        return conversationLines;
    }

    private static ConversationLine GetConversationLine(DataRow row)
    {
        return new ConversationLine
        {
            HashInteractionId = row.ItemArray[0]?.ToString(),
            HashCaseId = row.ItemArray[1]?.ToString(),
            ContactType = Enum.TryParse(row.ItemArray[2]?.ToString(), out ContactType contactType) ? contactType : ContactType.Unrecognized,
            Message = new Message
            {
                UtcTimeStamp = DateTime.TryParse(row.ItemArray[3]?.ToString(), out DateTime utcTimeStamp) ? utcTimeStamp : DateTime.MinValue,
                Content = row.ItemArray[4]?.ToString()
            }
        };
    }

    private static Conversation GetConverstation(List<ConversationLine> conversationLines, string? fileName)
    {
        var conversation = new Conversation
        {
            SessionId = fileName,
            UtcStart = conversationLines.Min(time => time.Message.UtcTimeStamp),
            UtcEnd = conversationLines.Max(time => time.Message.UtcTimeStamp)
        };

        var orderAndGroupedLines = conversationLines
            .OrderBy(cl => cl.ContactType)
            .GroupBy(cl => cl.ContactType);

        foreach (var userLine in orderAndGroupedLines)
            conversation.Contacts.Add(GetContact(userLine));

        return conversation;
    }

    private static Contact GetContact(IGrouping<ContactType, ConversationLine> userLine)
    {
        var contactType = GetType(userLine.FirstOrDefault().ContactType);
        var isAgent = contactType == CSVToJson.Models.Type.agent;

        var contact = new Contact
        {
            Type = contactType,
            ReferenceId = isAgent ? "SocialResponseAgent" : "SocialResponseCustomer",
            FirstName = isAgent ? "Social" : "SocialResponseCustomer",
            LastName = isAgent ? "Response" : string.Empty
        };

        foreach (var message in userLine.Select(ul => ul.Message))
            contact.Messages.Add(message);

        return contact;
    }

    private static CSVToJson.Models.Type GetType(ContactType contactType)
    {
        return contactType == ContactType.Advisor ?
                        CSVToJson.Models.Type.agent :
                        contactType == ContactType.Customer ?
                        CSVToJson.Models.Type.customer :
                        CSVToJson.Models.Type.invalid;
    }

    private static void WriteConversationToJsonFile(Conversation conversation, string jsonDestination)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        var jsonString = JsonSerializer.Serialize(conversation, options);

        File.WriteAllText(jsonDestination, jsonString);
    }
}