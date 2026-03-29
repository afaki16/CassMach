using CassMach.Application.Common.Results;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Models;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Errors.Commands.AcceptSolution
{
    public class AcceptSolutionCommandHandler : IRequestHandler<AcceptSolutionCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AcceptSolutionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(AcceptSolutionCommand request, CancellationToken cancellationToken)
        {
            var solutions = await _unitOfWork.ErrorSolutions.GetByConversationId(request.ConversationId, request.UserId);

            if (solutions == null || solutions.Count == 0)
                return Result<bool>.Failure(Error.Failure(ErrorCode.NotFound, "Conversation not found"));

            var targetSolution = solutions.FirstOrDefault(s => s.AttemptNumber == request.AttemptNumber);
            if (targetSolution == null)
                return Result<bool>.Failure(Error.Failure(ErrorCode.NotFound, "Attempt not found"));

            foreach (var solution in solutions)
            {
                solution.IsAccepted = solution.AttemptNumber == request.AttemptNumber;
                _unitOfWork.ErrorSolutions.Update(solution);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
