using MediatR;

namespace DeepMatch.Application.Features.Reports.Commands.ReportUser;

public record ReportUserCommand(Guid ReportedUserId, string Reason) : IRequest;


