namespace LeafMap_Insights.Services
{
    public interface IQRCodeService
    {
        string GenerateQRCode(string url);
        byte[] GenerateQRCodeBytes(string url);
    }
}
