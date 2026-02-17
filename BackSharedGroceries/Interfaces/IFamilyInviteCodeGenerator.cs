namespace BackSharedGroceries.Interfaces
{
    public interface IFamilyInviteCodeGenerator
    {
        Task<string> GenerateInviteCode();
    }
}