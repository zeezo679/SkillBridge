using AutoMapper;
using Microsoft.Extensions.Configuration;


namespace SoftBridge.Services.Resolver
{
    // we make the class generic to be reusable for any source (Model) and destination types (Dto),
    // as long as the source member is a string (the image path) and
    // the destination member is also a string (the full URL)
    public class PictureUrlResolver<TSource, TDestination>(IConfiguration _configuration)
            : IMemberValueResolver<TSource, TDestination, string, string>
    {
        public string Resolve(TSource source, TDestination destination, string sourceMember, string destMember, ResolutionContext context)
        {
            // 1. if the source member (the image path) is null or empty,
            // we return an empty string to avoid returning a broken URL
            if (string.IsNullOrEmpty(sourceMember))
            {
                return string.Empty;
            }

            // 2. get the base URL from the configuration (appsettings.json) using
            var baseUrl = _configuration["ApiBaseUrl"];

            // 3. combine the base URL with the source member (the image path) to form the full URL
            return $"{baseUrl}/{sourceMember.TrimStart('/')}";
        }
    }
}
