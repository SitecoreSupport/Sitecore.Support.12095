using System;
using System.Web.UI.HtmlControls;
using Sitecore.Pipelines;
using Sitecore.XA.Foundation.RenderingVariants.Fields;
using Sitecore.XA.Foundation.Variants.Abstractions.Fields;
using Sitecore.XA.Foundation.Variants.Abstractions.Models;
using Sitecore.XA.Foundation.Variants.Abstractions.Pipelines.RenderVariantField;

namespace Sitecore.Support.XA.Foundation.RenderingVariants.Pipelines.RenderVariantField
{
  public class RenderSection : RenderRenderingVariantFieldProcessor
  {
    public override Type SupportedType => typeof(VariantSection);
    public override RendererMode RendererMode => RendererMode.Html;
    public override void RenderField(RenderVariantFieldArgs args)
    {
      VariantSection variantSection = args.VariantField as VariantSection;
      if (variantSection != null)
      {
        HtmlGenericControl tag = new HtmlGenericControl(string.IsNullOrWhiteSpace(variantSection.Tag) ? "div" : variantSection.Tag);
        AddClass(tag, variantSection.CssClass);
        AddWrapperDataAttributes(variantSection, args, tag);

        foreach (BaseVariantField variantField in variantSection.SectionFields)
        {
          RenderVariantFieldArgs renderVariantArgs = new RenderVariantFieldArgs
          {
            VariantField = variantField,
            Item = args.Item,
            HtmlHelper = args.HtmlHelper,
            IsControlEditable = args.IsControlEditable,
            IsFromComposite = args.IsFromComposite,
            RendererMode = args.RendererMode
          };
          CorePipeline.Run("renderVariantField", renderVariantArgs);
          if (renderVariantArgs.ResultControl != null)
          {
            tag.Controls.Add(renderVariantArgs.ResultControl);
          }
        }

        args.ResultControl = variantSection.IsLink ? InsertHyperLink(tag, args.Item, variantSection.LinkAttributes, variantSection.LinkField, false, args.HrefOverrideFunc) : tag;
        args.Result = RenderControl(args.ResultControl);
      }
    }
  }
}