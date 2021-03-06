﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Modularity;
using WDE.Common;
using WDE.Common.Database;
using WDE.Common.History;
using WDE.Common.Managers;
using WDE.SmartScriptEditor.Data;
using WDE.SmartScriptEditor.Editor.ViewModels;
using WDE.SmartScriptEditor.Editor.Views;
using Prism.Ioc;
using Prism.Events;
using SmartFormat;
using WDE.Common.Providers;
using WDE.Common.DBC;
using WDE.Common.Solution;
using WDE.SmartScriptEditor.Providers;
using WDE.Module;

namespace WDE.SmartScriptEditor
{
    public class SmartScriptModule : ModuleBase
    {
        public SmartScriptModule()
        {
            Smart.Default.Parser.UseAlternativeEscapeChar('\\');
        }
    }
}
