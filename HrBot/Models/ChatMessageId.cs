namespace HrBot.Models
{
    public class ChatMessageId
    {
        public long ChatId { get; }

        public int MessageId { get; }

        public ChatMessageId(long chatId, int messageId)
        {
            ChatId = chatId;
            MessageId = messageId;

        }
    }
}
