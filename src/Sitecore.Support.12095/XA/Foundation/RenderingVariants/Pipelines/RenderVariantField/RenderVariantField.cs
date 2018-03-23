using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Pipelines;
using Sitecore.Web.UI.WebControls;
using Sitecore.XA.Foundation.RenderingVariants.Fields;
using Sitecore.XA.Foundation.Variants.Abstractions.Fields;
using Sitecore.XA.Foundation.Variants.Abstractions.Models;
using Sitecore.XA.Foundation.Variants.Abstractions.Pipelines.RenderVariantField;

namespace Sitecore.Support.XA.Foundation.RenderingVariants.Pipelines.RenderVariantField
{
  public class RenderVariantField : RenderRenderingVariantFieldProcessor
  {
    public override Type SupportedType => typeof(VariantField);
    public override RendererMode RendererMode => RendererMode.Html;
    public override void RenderField(RenderVariantFieldArgs args)
    {
      VariantField variantField = args.VariantField as VariantField;
      if (variantField != null)
      {
        BaseVariantField fallbackVariantField = ResolveFallback(variantField, args.Item, args.IsControlEditable);
        if (fallbackVariantField is VariantField)
        {
          args.ResultControl = Render(fallbackVariantField as VariantField, args);
        }
        else
        {
          //field fallback was resolved and we are rendering different field now (or the same if no fallback was defined)
          args.ResultControl = RenderFallbackField(fallbackVariantField, args.Item, args.IsControlEditable, args.IsFromComposite, args.RendererMode);
        }

        args.Result = RenderControl(args.ResultControl);
      }
    }

    protected virtual Control Render(VariantField variantField, RenderVariantFieldArgs args)
    {
      if (string.IsNullOrEmpty(variantField.FieldName))
      {
        return new LiteralControl();
      }

      if (IsEmptyFieldToRender(variantField, args.Item) || IsFromSnippedAndEmpty(variantField, args.Item, args.IsControlEditable))
      {
        //do not render empty fields if variant is configured to this or we have control which comes form the partial design
        return new LiteralControl();
      }

      Control control;

      //first check if we have such field in target item and if not display message that such field wasn't found - but just in edit mode
      if (args.Item.Fields[variantField.FieldName] != null)
      {
        control = CreateFieldRenderer(variantField, args.Item, args.IsControlEditable, args.IsFromComposite);
      }
      else
      {
        if (args.IsControlEditable && Context.PageMode.IsExperienceEditorEditing)
        {
          control = GetVariantFieldNameLiteral(variantField);
        }
        else
        {
          return new LiteralControl();
        }
      }

      //protection for rendering link inside of a link
      variantField.IsLink = ProtectLink(variantField.FieldName, variantField.IsLink, args.Item);

      control = HandleAffixAndLinkCreation(control,
          args.Item,
          variantField.Prefix,
          variantField.Suffix,
          variantField.IsLink,
          variantField.IsDownloadLink,
          variantField.IsPrefixLink,
          variantField.IsSuffixLink,
          variantField.LinkAttributes,
          variantField.LinkField,
          args.HrefOverrideFunc);

      if (!string.IsNullOrWhiteSpace(variantField.Tag))
      {
        HtmlGenericControl tag = new HtmlGenericControl(variantField.Tag);
        AddClass(tag, $"{variantField.CssClass} {GetFieldCssClass(variantField.FieldName)}".Trim());
        AddWrapperDataAttributes(variantField, args, tag);
        MoveControl(control, tag);
        control = tag;
      }

      return control;

    }

    protected virtual void AddWrapperDataAttributes(VariantField variantField, RenderVariantFieldArgs args, HtmlGenericControl tag)
    {
      if (variantField.DataAttributes.Count > 0)
      {
        string fieldType = args.Item.Fields[variantField.FieldName].Type;
        if (fieldType != "Image" && fieldType != "General Link")
        {
          base.AddWrapperDataAttributes(variantField, args, tag);
        }
      }
    }

    protected virtual Control CreateFieldRenderer(VariantField variantField, Item item, bool isControlEditable, bool isFromComposite)
    {
      FieldRenderer fieldRender = new FieldRenderer
      {
        Item = item,
        FieldName = variantField.FieldName,
        DisableWebEditing = !isControlEditable || (isControlEditable && !variantField.UseFieldRenderer)
      };
      if (variantField is VariantDate)
      {
        if (!Context.PageMode.IsNormal && isFromComposite && isControlEditable)
        {
          fieldRender.Parameters = $"Format={(variantField as VariantDate).DateFormat} skipcommonbuttons=\"{isFromComposite}\"";
        }
        else
        {
          fieldRender.Parameters = $"Format={(variantField as VariantDate).DateFormat}";
        }
      }
      else
      {
        if (!Context.PageMode.IsNormal && isFromComposite && isControlEditable)
        {
          fieldRender.Parameters = $"skipcommonbuttons={isFromComposite}";
        }
      }

      HandleAdditionalFieldRendererParameters(variantField, item, fieldRender);

      return fieldRender;
    }

    protected virtual void HandleAdditionalFieldRendererParameters(VariantField variantField, Item item, FieldRenderer control)
    {
      string str = string.Format("{0}={1}&{2}={3}", new object[4]
      {
        (object) "data-variantitemid",
        (object) item.ID,
        (object) "data-variantfieldname",
        (object) HttpUtility.UrlEncode(variantField.FieldName ?? "")
      });
      foreach (KeyValuePair<string, string> variantAttribute in (IEnumerable<KeyValuePair<string, string>>)this.GetVariantAttributes((RenderingVariantFieldBase)variantField, item))
        str += string.Format("&{0}={1}", (object)variantAttribute.Key, (object)variantAttribute.Value);
      control.Parameters = string.Join("&", control.Parameters, str).Trim('&');
    }

    /// <summary>
    /// Checks if sometimes comes from partial design and the field is empty
    /// </summary>
    protected virtual bool IsFromSnippedAndEmpty(VariantField variantField, Item item, bool isControlEditable)
    {
      return !isControlEditable && string.IsNullOrWhiteSpace(item[variantField.FieldName]);
    }

    /// <summary>
    /// Check if variant is configured to render empty fields and if field is empty
    /// </summary>
    protected virtual bool IsEmptyFieldToRender(VariantField variantField, Item item)
    {
      return !variantField.RenderIfEmpty && string.IsNullOrWhiteSpace(item[variantField.FieldName]);
    }

    protected virtual Control GetVariantFieldNameLiteral(VariantField variantField)
    {
      //if we are editing partial design or normal page
      return new LiteralControl($"<span class=\"missing-field-hint\">{variantField.FieldName} field</span>");
    }

    protected virtual Control RenderFallbackField(BaseVariantField fallbackVariantField, Item item, bool isControlEditable, bool isFromComposite, RendererMode rendererMode)
    {
      RenderVariantFieldArgs renderVariantArgs = new RenderVariantFieldArgs(fallbackVariantField, item)
      {
        IsControlEditable = isControlEditable,
        IsFromComposite = isFromComposite,
        RendererMode = rendererMode
      };
      CorePipeline.Run("renderVariantField", renderVariantArgs);
      return renderVariantArgs.ResultControl;
    }

    protected virtual BaseVariantField ResolveFallback(VariantField variantField, Item item, bool isControlEditable)
    {
      if (variantField.FallbackFields.Any())
      {
        foreach (BaseVariantField fallbackVariantField in variantField.FallbackFields)
        {
          if (fallbackVariantField is VariantField)
          {
            VariantField fallback = fallbackVariantField as VariantField;
            bool isPageEdit = fallback.UseFieldRenderer && Context.PageMode.IsExperienceEditorEditing;
            Field field = item.Fields[fallback.FieldName];
            if (field != null)
            {
              //if we are in edit mode then field just have to exists
              if (isPageEdit && isControlEditable)
              {
                return fallback;
              }

              //if we are in preview or publish mode then field have to exist and must have value and have to be different then $name
              string fieldValue = field.GetValue(true);
              if (!string.IsNullOrWhiteSpace(fieldValue) && fieldValue != "$name")
              {
                return fallback;
              }
            }
          }
          else
          {
            //in case of Token or VariantText there is no need to check anything as it will be always rendered
            return fallbackVariantField;
          }
        }
      }
      return variantField;
    }
  }
}