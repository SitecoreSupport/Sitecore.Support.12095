using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Pipelines;
using Sitecore.Pipelines.RenderField;
using Sitecore.Resources.Media;
using System;
using System.Collections.Specialized;
using System.Web.UI;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Support.XA.Foundation.RenderingVariants.Pipelines.RenderVariantField
{
  public abstract class RenderRenderingVariantFieldProcessor : Sitecore.XA.Foundation.RenderingVariants.Pipelines.RenderVariantField.RenderRenderingVariantFieldProcessor
  {
    protected override Control CreateHyperLink(Item item, string linkFieldName, bool isDownloadLink, NameValueCollection attributes, Func<Item, string, string> hrefOverrideFunc)
    {
      UrlOptions urlOptions = (UrlOptions)UrlOptions.DefaultOptions.Clone();
      urlOptions.Language = item.Language;

      #region FIX 12095
      //urlOptions.LanguageEmbedding = LanguageEmbedding.AsNeeded;
      #endregion

      if (hrefOverrideFunc != null)
      {
        string result = hrefOverrideFunc(item, linkFieldName);
        if (result != null)
        {
          return CreateHyperLink(result, item, isDownloadLink, attributes);
        }
      }

      if (!string.IsNullOrWhiteSpace(linkFieldName))
      {
        Field field = item.Fields[linkFieldName];
        if (field != null)
        {
          CustomField customField = FieldTypeManager.GetField(field);
          if (customField is FileField)
          {
            FileField fileField = item.Fields[linkFieldName];
            string fileUrl = fileField.MediaItem != null ? MediaManager.GetMediaUrl(fileField.MediaItem, new MediaUrlOptions { Language = fileField.MediaItem.Language }) : string.Empty;
            return CreateHyperLink(fileUrl, item, isDownloadLink, attributes);
          }
          if (customField is ImageField)
          {
            ImageField imageField = item.Fields[linkFieldName];
            string mediaUrl = imageField.MediaItem != null ? MediaManager.GetMediaUrl(imageField.MediaItem, new MediaUrlOptions { Language = imageField.MediaItem.Language }) : string.Empty;
            return CreateHyperLink(mediaUrl, item, isDownloadLink, attributes);
          }
          if (customField is ReferenceField)
          {
            ReferenceField referenceField = item.Fields[linkFieldName];
            urlOptions.Language = referenceField.TargetItem.Language;
            return CreateHyperLink(LinkManager.GetItemUrl(referenceField.TargetItem, urlOptions), item, isDownloadLink, attributes);
          }
          if (customField is LinkField)
          {
            RenderFieldArgs renderFieldArgs = new RenderFieldArgs
            {
              Item = item,
              FieldName = linkFieldName,
              Parameters =
                            {
                                ["haschildren"] = "true"
                            }
            };
            if (isDownloadLink)
            {
              renderFieldArgs.Parameters["download"] = "";
            }

            CorePipeline.Run("renderField", renderFieldArgs);
            return new RenderFieldControl(renderFieldArgs.Result);
          }
        }
      }

      if (item.IsMediaItem())
      {
        return CreateHyperLink(MediaManager.GetMediaUrl(item, new MediaUrlOptions { Language = item.Language }), item, isDownloadLink, attributes);
      }

      return CreateHyperLink(LinkManager.GetItemUrl(item, urlOptions), item, isDownloadLink, attributes);
    }
  }
}