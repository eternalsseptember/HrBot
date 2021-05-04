namespace HrBot.Models
{
    public class MessageInfo
    {
        public long ChatId { get; }

        public int MessageId { get; }


        public MessageInfo(long chatId, int messageId)
        {
            ChatId = chatId;
            MessageId = messageId;
        }
    }
}
