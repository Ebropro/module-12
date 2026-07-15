namespace Tms.Api.Dtos;
// Href = the URL to call
// Rel = the relationship (self, update, delete, enrollments, enroll)
// Method = the HTTP verb the client should use
public record LinkDto(string Href, string Rel, string Method);