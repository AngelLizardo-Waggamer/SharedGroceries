using System.Security.Claims;
using BackSharedGroceries.Data;
using Microsoft.EntityFrameworkCore;

namespace BackSharedGroceries.Middlewares
{
    /// <summary>
    /// Middleware to validate the DeviceId associated with the authenticated user.
    /// </summary>
    /// <param name="next"></param>
    public class DeviceSessionMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        /// <summary>
        /// Invoke the middleware to validate the deviceId of the authenticated user that is making the request. If the deviceId does not match the one stored for the user in the DB, 
        /// the request is rejected and the login process must be done again.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // Verify if the user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Extract user claims in the AuthController
                string? userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string? deviceId = context.User.FindFirst("DeviceId")?.Value;

                if (Guid.TryParse(userId, out Guid userIdParsed))
                {
                    // Check the actual device Id registered on the BD
                    var user = await dbContext.Users
                        .AsNoTracking()
                        .Select(u => new { u.Id, u.CurrentDeviceId})
                        .FirstOrDefaultAsync(u => u.Id == userIdParsed);

                    // If the user cannot be found or the device Ids do not match, reject the request
                    if (user == null || user.CurrentDeviceId.ToString() != deviceId)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Expired session or invalid device. Please log in again."
                        });
                        return;
                    }
                }
            }

            // If all the code above passes successfully, it means that the request auth is valid, so the flow can continue to the next middleware or controller.
            await _next(context);
        }
    }
}