using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Errors.Dtos;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Models;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Errors.Queries.GetConversation
{
    public class GetConversationQueryHandler : IRequestHandler<GetConversationQuery, Result<List<ErrorSolutionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetConversationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<ErrorSolutionDto>>> Handle(GetConversationQuery request, CancellationToken cancellationToken)
        {
            var attempts = await _unitOfWork.ErrorSolutions.GetByConversationId(request.ConversationId, request.UserId);

            if (attempts == null || attempts.Count == 0)
                return Result<List<ErrorSolutionDto>>.Failure(Error.Failure(ErrorCode.NotFound, "Konuşma bulunamadı"));

            var dtos = _mapper.Map<List<ErrorSolutionDto>>(attempts);
            return Result<List<ErrorSolutionDto>>.Success(dtos);
        }
    }
}
