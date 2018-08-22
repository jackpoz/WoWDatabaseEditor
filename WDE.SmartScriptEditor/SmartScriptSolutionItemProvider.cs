using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using WDE.Common;
using WDE.Common.Database;
using Prism.Ioc;
using WDE.Common.DBC;
using WDE.SmartScriptEditor.Data;
using Prism.Events;

namespace WDE.SmartScriptEditor
{
    public abstract class SmartScriptSolutionItemProvider : ISolutionItemProvider
    {
        private readonly string _name;
        private readonly string _desc;
        private readonly ImageSource _icon;
        private readonly SmartScriptType _type;
        
        protected readonly IEventAggregator eventAggregator;
        protected readonly ISmartFactory smartFactory;
        protected readonly IDatabaseProvider database;
        protected readonly ISpellStore spellStore;

        protected SmartScriptSolutionItemProvider(string name, string desc, string icon, SmartScriptType type, IEventAggregator eventAggregator, ISmartFactory smartFactory, IDatabaseProvider database, ISpellStore spellStore)
        {
            _name = name;
            _desc = desc;
            _type = type;
            _icon = new BitmapImage(new Uri($"/WDE.SmartScriptEditor;component/Resources/{icon}.png", UriKind.Relative));


            this.eventAggregator = eventAggregator;
            this.smartFactory = smartFactory;
            this.database = database;
            this.spellStore = spellStore;
        }

        public string GetName()
        {
            return _name;
        }

        public ImageSource GetImage()
        {
            return _icon;
        }

        public string GetDescription()
        {
            return _desc;
        }

        public abstract ISolutionItem CreateSolutionItem();

        //public virtual ISolutionItem CreateSolutionItem()
        //{
        //    return new SmartScriptSolutionItem(0, _type, null);
        //}
    }

    public abstract class B : ISolutionItemProvider
    {
        public abstract ISolutionItem CreateSolutionItem();
        public abstract string GetDescription();
        public abstract ImageSource GetImage();
        public abstract string GetName();
    }

    public class AAAA : SmartScriptSolutionItemProvider
    {
        public AAAA(IEventAggregator eventAggregator, ISmartFactory smartFactory, IDatabaseProvider database, ISpellStore spellStore) : base("as", "bd", null, SmartScriptType.Aura, eventAggregator, smartFactory, database, spellStore)
        {
        }

        public override ISolutionItem CreateSolutionItem()
        {
            return null;
        }
    }



    public class SmartScriptCreatureProvider : SmartScriptSolutionItemProvider
    {
        private readonly ICreatureEntryProviderService creatureEntryProvider;

        public SmartScriptCreatureProvider(ICreatureEntryProviderService creatureEntryProvider, IEventAggregator eventAggregator, ISmartFactory smartFactory, IDatabaseProvider database, ISpellStore spellStore)
            : base("Creature Script", "Script any npc in game.", "SmartScriptCreatureIcon", SmartScriptType.Creature, eventAggregator, smartFactory, database, spellStore)
        {
            this.creatureEntryProvider = creatureEntryProvider;
        }

        public override ISolutionItem CreateSolutionItem()
        {
            uint? entry = creatureEntryProvider.GetEntryFromService();
            if (!entry.HasValue)
                return null;
            return new SmartScriptSolutionItem((int)entry.Value, SmartScriptType.Creature, eventAggregator, smartFactory, database, spellStore);
        }
    }

    public class SmartScriptGameobjectProvider : SmartScriptSolutionItemProvider
    {
        private readonly IGameobjectEntryProviderService goProvider;

        public SmartScriptGameobjectProvider(IGameobjectEntryProviderService goProvider, IEventAggregator eventAggregator, ISmartFactory smartFactory, IDatabaseProvider database, ISpellStore spellStore)
            : base("Gameobject Script", "Create script for object, including transports.", "SmartScriptGameobjectIcon", SmartScriptType.GameObject, eventAggregator, smartFactory, database, spellStore)
        {
            this.goProvider = goProvider;
        }

        public override ISolutionItem CreateSolutionItem()
        {
            uint? entry = goProvider.GetEntryFromService();
            if (!entry.HasValue)
                return null;
            return new SmartScriptSolutionItem((int)entry.Value, SmartScriptType.GameObject, eventAggregator, smartFactory, database, spellStore);
        }
    }

    public class SmartScriptQuestProvider : SmartScriptSolutionItemProvider
    {
        private readonly IQuestEntryProviderService service;

        public SmartScriptQuestProvider(IQuestEntryProviderService service, IEventAggregator eventAggregator, ISmartFactory smartFactory, IDatabaseProvider database, ISpellStore spellStore)
            : base("Quest Script", "Write a script for quest: on accept, on reward.", "SmartScriptQuestIcon", SmartScriptType.Quest, eventAggregator, smartFactory, database, spellStore)
        {
            this.service = service;
        }

        public override ISolutionItem CreateSolutionItem()
        {
            uint? entry = service.GetEntryFromService();
            if (!entry.HasValue)
                return null;
            return new SmartScriptSolutionItem((int)entry.Value, SmartScriptType.Quest, eventAggregator, smartFactory, database, spellStore);
        }
    }

    public class SmartScriptAuraProvider : SmartScriptSolutionItemProvider
    {
        private readonly ISpellEntryProviderService service;

        public SmartScriptAuraProvider(ISpellEntryProviderService service, IEventAggregator eventAggregator, ISmartFactory smartFactory, IDatabaseProvider database, ISpellStore spellStore)
            : base("Aura Script", "Auras can have scripted several events: on apply, on remove, on periodic tick.", "SmartScriptAuraIcon", SmartScriptType.Aura, eventAggregator, smartFactory, database, spellStore)
        {
            this.service = service;
        }

        public override ISolutionItem CreateSolutionItem()
        {
            uint? entry = service.GetEntryFromService();
            if (!entry.HasValue)
                return null;
            return new SmartScriptSolutionItem((int)entry.Value, SmartScriptType.Spell, eventAggregator, smartFactory, database, spellStore);
        }
    }

    public class SmartScriptSpellProvider : SmartScriptSolutionItemProvider
    {
        private readonly ISpellEntryProviderService service;

        public SmartScriptSpellProvider(ISpellEntryProviderService service, IEventAggregator eventAggregator, ISmartFactory smartFactory, IDatabaseProvider database, ISpellStore spellStore)
            : base("Spell Script", "Create a new script for spell: this includes script for any existing effect in spell.", "SmartScriptSpellIcon", SmartScriptType.Spell, eventAggregator, smartFactory, database, spellStore)
        {
            this.service = service;
        }

        public override ISolutionItem CreateSolutionItem()
        {
            uint? entry = service.GetEntryFromService();
            if (!entry.HasValue)
                return null;
            return new SmartScriptSolutionItem((int)entry.Value, SmartScriptType.Spell, eventAggregator, smartFactory, database, spellStore);
        }
    }

    public class SmartScriptTimedActionListProvider : SmartScriptSolutionItemProvider
    {
        public SmartScriptTimedActionListProvider(IEventAggregator eventAggregator, ISmartFactory smartFactory, IDatabaseProvider database, ISpellStore spellStore)
            : base("Timed action list", "Timed action list contains list of actions played in time, this can be used to create RP events, cameras, etc.", "SmartScriptTimedActionListIcon", SmartScriptType.Timed, eventAggregator, smartFactory, database, spellStore)
        {
        }

        public override ISolutionItem CreateSolutionItem()
        {
            return new SmartScriptSolutionItem(0, SmartScriptType.AreaTrigger, eventAggregator, smartFactory, database, spellStore);
        }
    }
}
