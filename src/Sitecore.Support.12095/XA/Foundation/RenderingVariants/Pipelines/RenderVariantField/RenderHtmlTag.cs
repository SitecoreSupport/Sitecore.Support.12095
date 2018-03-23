using System;
using System.Web.UI.HtmlControls;
using Sitecore.Data.Items;
using Sitecore.XA.Foundation.RenderingVariants.Fields;
using Sitecore.XA.Foundation.Variants.Abstractions.Models;
using Sitecore.XA.Foundation.Variants.Abstractions.Pipelines.RenderVariantField;

namespace Sitecore.Support.XA.Foundation.RenderingVariants.Pipelines.RenderVariantField
{
  public class RenderHtmlTag : RenderRenderingVariantFieldProcessor
  {
    public override Type SupportedType => typeof(VariantHtmlTag);

    public override RendererMode RendererMode => RendererMode.Html;

    public override void RenderField(RenderVariantFieldArgs args)
    {
      var variantHtmlTag = args.VariantField as VariantHtmlTag;
      if (variantHtmlTag != null)
      {
        if (string.IsNullOrWhiteSpace(variantHtmlTag.Tag))
        {
          return;
        }

        var tag = new HtmlGenericControl(variantHtmlTag.Tag);
        AddClass(tag, variantHtmlTag.CssClass);
        AddWrapperDataAttributes(variantHtmlTag, args, tag);

        PopulateAdditionalControlAttributes((RenderingVariantFieldBase)args.VariantField, args.Item, tag);

        args.ResultControl = variantHtmlTag.IsLink ? InsertHyperLink(tag, args.Item, variantHtmlTag.LinkAttributes, variantHtmlTag.LinkField, false, args.HrefOverrideFunc) : tag;
        args.Result = RenderControl(args.ResultControl);
      }
    }

    protected virtual void PopulateAdditionalControlAttributes(RenderingVariantFieldBase variantField, Item item, HtmlControl control)
    {
      var attributes = GetVariantAttributes(variantField, item);
      foreach (var attribute in attributes)
      {
        control.Attributes.Add(attribute.Key, attribute.Value);
      }
    }
  }
}