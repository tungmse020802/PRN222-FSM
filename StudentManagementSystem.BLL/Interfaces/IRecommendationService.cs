using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.BLL.Interfaces;

public interface IRecommendationService
{
    Task<AIRecommendation?> GetLatestAsync(int studentId, int semesterId, CancellationToken cancellationToken = default);
    Task<ServiceResult<AIRecommendation>> GenerateAsync(int studentId, int userAccountId, int semesterId, CancellationToken cancellationToken = default);
}
