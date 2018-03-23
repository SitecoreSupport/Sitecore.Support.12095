using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Sitecore.XA.Foundation.RenderingVariants.Fields;
using Sitecore.XA.Foundation.Variants.Abstractions.Models;
using Sitecore.XA.Foundation.Variants.Abstractions.Pipelines.RenderVariantField;

namespace Sitecore.Support.XA.Foundation.RenderingVariants.Pipelines.RenderVariantField
{
  public class RenderText : RenderRenderingVariantFieldProcessor
  {
    public override Type SupportedType => typeof(VariantText);
    public override RendererMode RendererMode => RendererMode.Html;
    public override void RenderField(RenderVariantFieldArgs args)
    {
      VariantText variantText = args.VariantField as VariantText;
      if (variantText != null)
      {
        Control control = new LiteralControl(variantText.Text);

        if (variantText.IsLink)
        {
          control = InsertHyperLink(control, args.Item, variantText.LinkAttributes, variantText.LinkField, false, args.HrefOverrideFunc);
        }

        if (!string.IsNullOrWhiteSpace(variantText.Tag))
        {
          HtmlGenericControl tag = new HtmlGenericControl(variantText.Tag);
          AddClass(tag, variantText.CssClass);
          AddWrapperDataAttributes(variantText, args, tag);
          MoveControl(control, tag);
          control = tag;
        }

        args.ResultControl = control;
        args.Result = RenderControl(args.ResultControl);
      }
    }
  }
}