using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PictureBot
{
    public class PictureBotAccessors
    {

        public PictureBotAccessors(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }
        public static string PictureStateName { get; } = $"{nameof(PictureBotAccessors)}.PictureState";

        public IStatePropertyAccessor<PictureState> PictureState { get; set; }

        public ConversationState ConversationState { get; }

        public static string DialogStateName { get; } = $"{nameof(PictureBotAccessors)}.DialogState";

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }
    }
}
