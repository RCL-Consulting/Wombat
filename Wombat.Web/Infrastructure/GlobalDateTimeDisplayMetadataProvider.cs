using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Wombat.Web.Infrastructure
{
    public sealed class GlobalDateTimeDisplayMetadataProvider : IDisplayMetadataProvider
    {
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.ModelType == typeof(DateTime) || context.Key.ModelType == typeof(DateTime?))
            {
                context.DisplayMetadata.DisplayFormatString = "{0:yyyy-MM-dd HH:mm}";
                context.DisplayMetadata.NullDisplayText = "";
            }
        }
    }
}
