using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PunchPal.WPF.Controls
{
    public class DataGridTextTrimingColumn : DataGridTextColumn
    {
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var textBlock = base.GenerateElement(cell, dataItem) as TextBlock;
            if (textBlock != null)
            {
                textBlock.TextTrimming = TextTrimming.CharacterEllipsis;

                if (Binding is Binding binding)
                {
                    textBlock.SetBinding(TextBlock.ToolTipProperty, binding);
                }
            }
            return textBlock;
        }
    }
}
