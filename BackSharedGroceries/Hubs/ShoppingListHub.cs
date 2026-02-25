using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BackSharedGroceries.Hubs
{
    /// <summary>
    /// Real-time hub for shopping list updates.
    /// Each family gets its own group so events only reach the right users.
    /// </summary>
    [Authorize]
    public class ShoppingListHub : Hub
    {
        /// <summary>
        /// Called when a client connects.
        /// Joins the connection to the group matching the user's family.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var familyId = Context.User?.FindFirst("FamilyId")?.Value;

            if (!string.IsNullOrEmpty(familyId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, familyId);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects.
        /// Leaves the family group.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var familyId = Context.User?.FindFirst("FamilyId")?.Value;

            if (!string.IsNullOrEmpty(familyId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, familyId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
