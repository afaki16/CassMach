using AutoMapper;
using CassMach.Application.Features.Tenants.Commands.CreateTenant;
using CassMach.Application.Features.Tenants.Commands.UpdateTenant;
using CassMach.Application.Features.Tenants.Commands.DeleteTenant;
using CassMach.Application.Features.Tenants.Dtos;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Common.Interfaces.Repositories;
using CassMach.Application.Common.Results;
using CassMach.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CassMach.Domain.Common.Enums;

namespace CassMach.Application.Features.Tenants.Commands.UpdateTenant;

    public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result<TenantListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateTenantCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TenantListDto>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetTenantWithUsersAsync(request.Id);

        if (tenant == null)
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.NotFound,
                "Tenant not found"));
        }

        // İsim kontrolü (eğer değiştiyse)
        if (tenant.Name != request.Name && await _unitOfWork.Tenants.NameExistsAsync(request.Name))
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                "Tenant name already exists"));
        }

        // Domain kontrolü (eğer değiştiyse)
        if (!string.IsNullOrEmpty(request.Domain) &&
            tenant.Domain != request.Domain &&
            await _unitOfWork.Tenants.DomainExistsAsync(request.Domain))
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                "Tenant domain already exists"));
        }

        tenant.Name = request.Name;
        tenant.Description = request.Description;
        tenant.Domain = request.Domain;
        tenant.IsActive = request.IsActive;
        tenant.ContactEmail = request.ContactEmail;
        tenant.ContactPhone = request.ContactPhone;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync();

        var tenantDto = _mapper.Map<TenantListDto>(tenant);
        tenantDto.UserCount = tenant.Users?.Count ?? 0;
        return Result<TenantListDto>.Success(tenantDto);
    }
}
