﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Templates.Wizard.Host;
using System.Collections.ObjectModel;
using Microsoft.Templates.Wizard.Steps.Pages;
using Microsoft.Templates.Core;
using System.Windows.Input;
using Microsoft.Templates.Core.Mvvm;
using Microsoft.Templates.Core.Gen;

namespace Microsoft.Templates.Wizard.Steps.ConsumerFeatures
{
    public class ViewModel : StepViewModel
    {
        public ViewModel(WizardContext context) : base(context)
        {
        }

        public ICommand AddPageCommand => new RelayCommand(ShowAddFeatureDialog);
        public ICommand RemovePageCommand => new RelayCommand(RemoveFeature);

        public ObservableCollection<PageViewModel> Templates { get; } = new ObservableCollection<PageViewModel>();

        private PageViewModel _templateSelected;
        public PageViewModel TemplateSelected
        {
            get { return _templateSelected; }
            set
            {
                SetProperty(ref _templateSelected, value);
            }
        }

        public override string PageTitle => Strings.PageTitle;

        public override async Task InitializeAsync()
        {
            Templates.Clear();

            if (Context.State.ConsumerFeatures == null || Context.State.ConsumerFeatures.Count == 0)
            {
                AddFromLayout();
            }
            else
            {
                var selectedFeatures = Context.State.ConsumerFeatures
                                                        .Select(p => new PageViewModel(p.name, p.template));

                Templates.AddRange(selectedFeatures);
            }

            await Task.CompletedTask;
        }

        public override void SaveState()
        {
            Context.State.ConsumerFeatures.Clear();
            Context.State.ConsumerFeatures.AddRange(Templates.Select(t => (t.Name, t.Template)));
        }

        protected override Page GetPageInternal()
        {
            return new View();
        }

        private void AddFromLayout()
        {
            var projectTemplate = GenContext.ToolBox.Repo.Find(t => t.GetProjectType() == Context.State.ProjectType && t.GetFrameworkList().Any(f => f == Context.State.Framework));
            var layout = projectTemplate.GetLayout();

            foreach (var item in layout)
            {
                var template = GenContext.ToolBox.Repo.GetLayoutTemplate(item, Context.State.Framework);
                if (template != null && template.GetTemplateType() == TemplateType.ConsumerFeature)
                {
                    Templates.Add(new PageViewModel(item.name, template, item.@readonly));
                }
            }
        }

        private void ShowAddFeatureDialog()
        {
            var dialog = new NewConsumerFeature.NewConsumerFeatureDialog(Context, Templates)
            {
                Owner = Context.Host
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                Templates.Add(new PageViewModel(dialog.Result.name, dialog.Result.template));
            }
        }

        private void RemoveFeature()
        {
            if (TemplateSelected != null && !TemplateSelected.Readonly)
            {
                Templates.Remove(TemplateSelected);
            }
        }
    }
}
