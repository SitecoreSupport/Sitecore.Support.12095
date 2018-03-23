using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Sitecore.Data.Items;
using Sitecore.Pipelines;
using Sitecore.XA.Foundation.RenderingVariants.Fields;
using Sitecore.XA.Foundation.Variants.Abstractions.Models;
using Sitecore.XA.Foundation.Variants.Abstractions.Pipelines.RenderVariantField;
using Sitecore.XA.Foundation.Variants.Abstractions.Pipelines.ResolveVariantTokens;

namespace Sitecore.Support.XA.Foundation.RenderingVariants.Pipelines.RenderVariantField
{
  public class RenderToken : RenderRenderingVariantFieldProcessor
  {
    public override Type SupportedType => typeof(VariantToken);
    public override RendererMode RendererMode => RendererMode.Html;
    public override void RenderField(RenderVariantFieldArgs args)
    {
      VariantToken variantToken = args.VariantField as VariantToken;
      if (variantToken != null)
      {
        Control control = HandleAffixAndLinkCreation(ResolveVariantTokens(variantToken, args.Item),
                                                     args.Item,
                                                     variantToken.Prefix,
                                                     variantToken.Suffix,
                                                     variantToken.IsLink,
                                                     variantToken.IsDownloadLink,
                                                     variantToken.IsPrefixLink,
                                                     variantToken.IsSuffixLink,
                                                     variantToken.LinkAttributes,
                                                     variantToken.LinkField,
                                                     args.HrefOverrideFunc);

        if (!string.IsNullOrWhiteSpace(variantToken.Tag))
        {
          HtmlGenericControl tag = new HtmlGenericControl(variantToken.Tag);
          AddClass(tag, $"{variantToken.CssClass} {GetFieldCssClass(variantToken.GetClearTokenName())}".Trim());
          AddWrapperDataAttributes(variantToken, args, tag);
          MoveControl(control, tag);
          control = tag;
        }

        args.ResultControl = control;
        args.Result = RenderControl(args.ResultControl);
      }
    }

    protected virtual Control ResolveVariantTokens(VariantToken variantToken, Item item)
    {
      ResolveVariantTokensArgs arguments = new ResolveVariantTokensArgs(variantToken.Token, item, "span");
      CorePipeline.Run("resolveVariantTokens", arguments);
      return arguments.ResultControl;
    }
  }
}