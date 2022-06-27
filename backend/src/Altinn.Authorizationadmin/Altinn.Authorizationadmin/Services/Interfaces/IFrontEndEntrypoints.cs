namespace Altinn.AuthorizationAdmin.Services
{
    public interface IFrontEndEntrypoints
    {
        String GetCSSEntrypoint();
        String GetJSEntrypoint();
    }
}
