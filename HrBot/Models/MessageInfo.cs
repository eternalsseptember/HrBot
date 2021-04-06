namespace HrBot.Models
{
    public class MessageInfo
    {
        public MessageInfo(long chatId, int messageId)
        {
            ChatId = chatId;
            MessageId = messageId;
        }


        public long ChatId { get; }

        public int MessageId { get; }
    }
}
