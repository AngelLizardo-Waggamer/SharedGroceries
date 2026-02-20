using System.Security.Claims;
using BackSharedGroceries.Common;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;
using BackSharedGroceries.Helpers;
using BackSharedGroceries.Interfaces;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Interfaces.Services;
using BackSharedGroceries.Models;
using BackSharedGroceries.Repositories;

namespace BackSharedGroceries.Services
{
    /// <summary>
    /// Service implementation for family-related operations.
    /// </summary>
    public class FamilyService : IFamilyService
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly IFamilyInviteCodeGenerator _inviteCodeGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FamilyService(IFamilyRepository familyRepository, IFamilyInviteCodeGenerator inviteCodeGenerator, IHttpContextAccessor httpContextAccessor)
        {
            _familyRepository = familyRepository;
            _inviteCodeGenerator = inviteCodeGenerator;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Creates a new family and associates the user as a member.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Service result containing the created family response.</returns>
        public async Task<ServiceResult<FamilyResponse>> CreateFamilyAsync(CreateFamilyRequest request)
        {

            // Get the user ID from the HTTP context
            // It first gets the user Guid before the operations to ensure it is available. If it is
            // not available it returns a bad request result.
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult<FamilyResponse>.BadRequest(ex.Message);
            }

            // Check if user already belongs to a family
            var currentFamilyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (currentFamilyId != null)
            {
                return ServiceResult<FamilyResponse>.BadRequest("User already belongs to a family.");
            }
            
            // Generate a unique invite code for the family
            string inviteCode = await _inviteCodeGenerator.GenerateInviteCode();

            // Create the family entity
            Family newFamily = new()
            {
              FamilyName = request.FamilyName,
              FamilyInviteCode = inviteCode
            };

            // Save the family to the database
            await _familyRepository.CreateFamilyAsync(newFamily);

            // Assign the user that created the family to it
            await _familyRepository.UpdateUserFamilyAsync(userId, newFamily.FamilyId);

            // Return the created family information
            return ServiceResult<FamilyResponse>.Ok(new FamilyResponse
            {
                Id = newFamily.FamilyId,
                Name = newFamily.FamilyName,
                InviteCode = newFamily.FamilyInviteCode  
            });
        }

        /// <summary>
        /// Allows a user to join an existing family using an invite code.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Service result indicating the success or failure of the join operation.</returns>
        public async Task<ServiceResult> JoinFamilyAsync(JoinFamilyRequest request)
        {

            // Get the user ID from the HTTP context
            // It first gets the user Guid before the operations to ensure it is available. If it is
            // not available it returns a bad request result.
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult.BadRequest(ex.Message);
            }

            // Check if user already belongs to a family
            var currentFamilyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (currentFamilyId != null)
            {
                return ServiceResult.BadRequest("User already belongs to a family.");
            }

            // Normalize the invite code (remove hyphens and convert to uppercase)
            string normalizedCode = request.InviteCode.Replace("-", "").ToUpper();

            // Obtain the family entity using the invite code
            Family? family = await _familyRepository.GetByInviteCodeAsync(normalizedCode);

            // If there is no family with the provided invite code, return not found
            if (family == null) {
                return ServiceResult.NotFound("Family with the provided invite code does not exist.");
            }

            // Assign the user to the found family
            await _familyRepository.UpdateUserFamilyAsync(userId, family.FamilyId);
            return ServiceResult.Ok();
        }

        /// <summary>
        /// Allows an authenticated user to leave their current family.
        /// </summary>
        /// <returns>Service result indicating the success or failure of the leave operation.</returns>
        public async Task<ServiceResult> LeaveFamilyAsync()
        {
            // Get the user ID from the HTTP context
            // It first gets the user Guid before the operations to ensure it is available. If it is
            // not available it returns a bad request result.
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult.BadRequest(ex.Message);
            }

            // Check if the user is currently part of a family
            // If FamilyId is null, the user is not in any family
            Guid? currentFamilyId = await _familyRepository.GetUserFamilyIdAsync(userId);

            if (currentFamilyId == null)
            {
                return ServiceResult.BadRequest("User is not part of any family.");
            }

            // Remove the user from their current family by setting FamilyId to null
            await _familyRepository.RemoveUserFromFamilyAsync(userId);

            // Check if this was the last member of the family
            var remainingMembers = await _familyRepository.GetFamilyMemberCountAsync(currentFamilyId.Value);
            if (remainingMembers == 0)
            {
                // Delete the family if no members remain
                await _familyRepository.DeleteFamilyAsync(currentFamilyId.Value);
            }

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Returns the user Guid parsed from the HttpContext claims.
        /// </summary>
        /// <returns>User Guid object</returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext or User is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the NameIdentifier claim is missing.</exception>
        /// <exception cref="FormatException">Thrown when the NameIdentifier claim is not a valid GUID.</exception>
        private Guid GetUserId()
        {
            // Check if the context exists
            var user = (_httpContextAccessor.HttpContext?.User) ?? throw new InvalidOperationException("User context is unavailable.");

            // Try to find the claim
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                throw new UnauthorizedAccessException("User ID claim is missing from the token.");
            }

            // Parse the claim value to a Guid
            if (!Guid.TryParse(userIdClaim.Value, out Guid userGuid))
            {
                throw new FormatException("The User ID claim is not a valid GUID.");
            }

            return userGuid;
        }
    }
}