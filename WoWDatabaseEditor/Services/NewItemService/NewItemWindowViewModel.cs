﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using WDE.Common;
using Prism.Ioc;

namespace WoWDatabaseEditor.Services.NewItemService
{
    [WDE.Module.Attributes.AutoRegister]
    public class NewItemWindowViewModel : BindableBase, INewItemWindowViewModel
    {
        public ObservableCollection<NewItemPrototypeInfo> ItemPrototypes { get; private set; }

        private NewItemPrototypeInfo? _selectedPrototype;
        public NewItemPrototypeInfo? SelectedPrototype
        {
            get { return _selectedPrototype; }
            set { SetProperty(ref _selectedPrototype, value); }
        }
        
        public NewItemWindowViewModel(IEnumerable<ISolutionItemProvider> items)
        {
            ItemPrototypes = new ObservableCollection<NewItemPrototypeInfo>();

            foreach (var item in items)
            {
                ItemPrototypes.Add(new NewItemPrototypeInfo(item));
            }
        }
    }
}
