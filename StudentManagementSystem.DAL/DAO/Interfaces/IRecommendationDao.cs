using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.DAO.Interfaces;

public interface IRecommendationDao
{
    Task<AIRecommendation?> GetLatestAsync(int studentId, int semesterId, CancellationToken cancellationToken = default);
    Task SaveAsync(AIRecommendation recommendation, CancellationToken cancellationToken = default);
}
