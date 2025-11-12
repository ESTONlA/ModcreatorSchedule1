using System.Windows;
using System.Windows.Controls;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Template selector for wizard steps
    /// </summary>
    public class WizardStepTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? Step1Template { get; set; }
        public DataTemplate? Step2Template { get; set; }
        public DataTemplate? Step3Template { get; set; }
        public DataTemplate? Step4Template { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            int step;
            
            // Item could be CurrentStep (int) or ViewModel
            if (item is int stepNumber)
            {
                step = stepNumber;
            }
            else if (item is ViewModels.FirstStartWizardViewModel viewModel)
            {
                step = viewModel.CurrentStep;
            }
            else
            {
                return null;
            }

            return step switch
            {
                1 => Step1Template,
                2 => Step2Template,
                3 => Step3Template,
                4 => Step4Template,
                _ => null
            };
        }
    }
}

