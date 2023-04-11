using System;
using System.Windows.Media;
using System.Text.Json;

namespace MessangerServer
{
    public class Message
    {
        public bool IsService { get; set; }
        public string? Sender { get; set; }
        public string? Recepient { get; set; }
        public string? Text { get; set; }
        public Color Color { get; set; }

        public Message(bool isService,
            string sender,
            string recepient,
            string text,
            Color color)
        {
            IsService = isService;
            Sender = sender;
            Recepient = recepient;
            Text = text;
            Color = color;
        }
        public Message()
        {
            IsService = false;
            Sender = "Host";
            Recepient = "@all";
            Text = "some text";
            Color = Colors.Black;
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        public static Message Deserialize(string json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            else
            {
                Message? msg = JsonSerializer.Deserialize<Message>(json);
                if (msg == null)
                {
                    throw new ArgumentNullException(nameof(msg));
                }
                else
                    return msg;
            }
        }
    }
}
