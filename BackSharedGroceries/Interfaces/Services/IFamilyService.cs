using BackSharedGroceries.Common;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;

namespace BackSharedGroceries.Interfaces.Services
{
    /// <summary>
    /// Service interface for family-related operations.
    /// </summary>
    public interface IFamilyService
    {
        /// <summary>
        /// Creates a new family and associates the user as a member.
        /// </summary>
        /// <param name="request">The request object containing family creation details.</param>
        /// <returns>Service result containing the created family response.</returns>
        Task<ServiceResult<FamilyResponse>> CreateFamilyAsync(CreateFamilyRequest request);

        /// <summary>
        /// Allows a user to join an existing family using an invite code.
        /// </summary>
        /// <param name="request">The request object containing the invite code.</param>
        /// <returns>Service result indicating the success or failure of the join operation.</returns>
        Task<ServiceResult<FamilyResponse>> JoinFamilyAsync(JoinFamilyRequest request);

        /// <summary>
        /// Allows an authenticated user to leave their current family.
        /// </summary>
        /// <returns>Service result indicating the success or failure of the leave operation.</returns>
        Task<ServiceResult> LeaveFamilyAsync();

        /// <summary>
        /// Retrieves the family information for the authenticated user.
        /// </summary>
        /// <returns>Service result containing the user's family information.</returns>
        Task<ServiceResult<FamilyResponse>> GetUserFamilyAsync();
    }
}