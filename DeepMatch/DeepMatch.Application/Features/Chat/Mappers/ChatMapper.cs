using DeepMatch.Application.Features.Chat.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Chat.Mappers;

public static class ChatMapper
{
    public static ChatInfoDto ToChatInfoDto(Match match, Guid currentUserId)
    {
        var isCurrentFirstUser = match.User1Id == currentUserId;

        var currentUserName = isCurrentFirstUser ? match.User1.UserName : match.User2.UserName;
        var matchedUserName = isCurrentFirstUser ? match.User2.UserName : match.User1.UserName;

        var currentAnswer = isCurrentFirstUser ? match.CatalystAnswer1 : match.CatalystAnswer2;
        var matchedAnswer = isCurrentFirstUser ? match.CatalystAnswer2 : match.CatalystAnswer1;

        return new ChatInfoDto(
            match.Id,
            currentUserName,
            matchedUserName,
            currentAnswer.Question.Text,
            currentAnswer.Text,
            matchedAnswer.Question.Text,
            matchedAnswer.Text);
    }
}
