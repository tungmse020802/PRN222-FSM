using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Data;
using StudentManagementSystem.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementSystem.DAL.DAO;

public sealed class RecommendationDao(IDbContextFactory<StudentManagementDbContext> contextFactory) : IRecommendationDao
{
    public async Task<AIRecommendation?> GetLatestAsync(int studentId, int semesterId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.AIRecommendations.AsNoTracking()
            .Include(x => x.Semester)
            .FirstOrDefaultAsync(x => x.StudentId == studentId && x.SemesterId == semesterId, cancellationToken);
    }

    public async Task SaveAsync(AIRecommendation recommendation, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await context.AIRecommendations
            .FirstOrDefaultAsync(x => x.StudentId == recommendation.StudentId && x.SemesterId == recommendation.SemesterId, cancellationToken);

        if (existing is null)
        {
            context.AIRecommendations.Add(recommendation);
        }
        else
        {
            existing.RiskLevel = recommendation.RiskLevel;
            existing.RecommendedCredits = recommendation.RecommendedCredits;
            existing.Summary = recommendation.Summary;
            existing.RecommendedSubjects = recommendation.RecommendedSubjects;
            existing.AvoidSubjects = recommendation.AvoidSubjects;
            existing.GeneratedAt = recommendation.GeneratedAt;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
