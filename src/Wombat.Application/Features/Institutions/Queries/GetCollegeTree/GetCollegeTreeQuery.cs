using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetCollegeTree;

/// <summary>
/// The College's tree (T394, flow 16, Q1): the College, each of its specialities, and each speciality's sub-specialities
/// with the name of the default ladder each uses. Null for an unknown College, and for one the caller cannot open
/// (<c>CanAccessCollege</c>): the page then reads "College unavailable" (Q7), never an empty list offering Create.
/// </summary>
/// <remarks>
/// The tree's page reads it, and so do a speciality's and a sub-speciality's record pages, for the College and the
/// speciality they sit in (their trail, subtitle and the record itself), so a record another College holds reads as one
/// that does not exist.
/// </remarks>
public sealed record GetCollegeTreeQuery(int CollegeId, ClaimsPrincipal Principal) : IRequest<CollegeTreeDto?>;
