using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.Repositories.Interfaces;

public interface IRecommendationRepository
{
    Task<AIRecommendation?> GetLatestAsync(int studentId, int semesterId, CancellationToken cancellationToken = default);
    Task SaveAsync(AIRecommendation recommendation, CancellationToken cancellationToken = default);
}
