namespace DeepMatch.Application.Features.Chat.Common;

public record MessageDto(
    Guid Id, 
    Guid MatchId, 
    string Content, 
    Guid? SenderUserId, 
    DateTime Timestamp, 
    bool IsIcebreaker = false);
