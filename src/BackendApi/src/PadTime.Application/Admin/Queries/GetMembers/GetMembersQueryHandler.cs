using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Common.Models;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Queries.GetMembers;

public sealed class GetMembersQueryHandler(
    IMemberRepository members,
    IOrganizerDebtRepository debts,
    ISiteRepository sites)
    : IRequestHandler<GetMembersQuery, Result<PagedResult<MemberListItemDto>>>
{
    public async Task<Result<PagedResult<MemberListItemDto>>> Handle(
        GetMembersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await members.GetPagedAsync(
            request.Page, request.PageSize,
            request.Category, request.IsActive, request.Search,
            cancellationToken);

        if (items.Count == 0)
            return new PagedResult<MemberListItemDto>([], request.Page, request.PageSize, totalCount);

        var memberIds = items.Select(m => m.Id).ToList();

        var matchCounts = await members.GetMatchCountsAsync(memberIds, cancellationToken);

        var allDebts = await debts.GetAllActiveAsync(cancellationToken);
        var debtsByMember = allDebts.ToDictionary(d => d.MemberId, d => d.AmountCents);

        var siteIds = items.Where(m => m.SiteId.HasValue).Select(m => m.SiteId!.Value).Distinct().ToList();
        var siteNames = new Dictionary<Guid, string>();
        foreach (var siteId in siteIds)
        {
            var site = await sites.GetByIdAsync(siteId, cancellationToken);
            if (site is not null)
                siteNames[siteId] = site.Name;
        }

        var dtos = items.Select(m => new MemberListItemDto(
            m.Id,
            m.Subject,
            m.Matricule.Value,
            m.Category,
            m.SiteId,
            m.SiteId.HasValue && siteNames.TryGetValue(m.SiteId.Value, out var name) ? name : null,
            m.IsActive,
            m.CreatedAtUtc,
            matchCounts.GetValueOrDefault(m.Id, 0),
            debtsByMember.GetValueOrDefault(m.Id, 0)
        )).ToList();

        return new PagedResult<MemberListItemDto>(dtos, request.Page, request.PageSize, totalCount);
    }
}
