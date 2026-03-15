using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.Repositories;

public sealed class RecommendationRepository(IRecommendationDao dao) : IRecommendationRepository
{
    public Task<AIRecommendation?> GetLatestAsync(int studentId, int semesterId, CancellationToken cancellationToken = default) => dao.GetLatestAsync(studentId, semesterId, cancellationToken);
    public Task SaveAsync(AIRecommendation recommendation, CancellationToken cancellationToken = default) => dao.SaveAsync(recommendation, cancellationToken);
}
