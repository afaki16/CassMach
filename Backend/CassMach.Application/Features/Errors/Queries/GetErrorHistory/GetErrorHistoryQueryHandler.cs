using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Errors.Dtos;
using CassMach.Domain.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Errors.Queries.GetErrorHistory
{
    public class GetErrorHistoryQueryHandler : IRequestHandler<GetErrorHistoryQuery, PagedResult<ErrorSolutionListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetErrorHistoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<ErrorSolutionListDto>> Handle(GetErrorHistoryQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await _unitOfWork.ErrorSolutions.GetUserHistoryCount(request.UserId, request.SearchTerm);
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = await _unitOfWork.ErrorSolutions.GetUserHistoryPaged(
                request.UserId, request.Page, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<IEnumerable<ErrorSolutionListDto>>(items);

            return PagedResult<ErrorSolutionListDto>.Success(dtos, request.Page, totalPages, totalCount);
        }
    }
}
