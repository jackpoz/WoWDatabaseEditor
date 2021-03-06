﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WDE.Common.Database;
using WDE.SmartScriptEditor.Data;
using Prism.Ioc;
using WDE.SmartScriptEditor.Editor.UserControls;

namespace WDE.SmartScriptEditor.Models
{
    public class SmartScript
    {
        public readonly ObservableCollection<SmartEvent> Events;
        public readonly int EntryOrGuid;
        public readonly SmartScriptType SourceType;
        private readonly ISmartFactory smartFactory;
        public event Action BulkEditingStarted = delegate {  };
        public event Action<string> BulkEditingFinished = delegate {  };

        public SmartScript(SmartScriptSolutionItem item, ISmartFactory smartFactory)
        {
            EntryOrGuid = item.Entry;
            SourceType = item.SmartType;
            Events = new ObservableCollection<SmartEvent>();
            this.smartFactory = smartFactory;
        }

        public void Load(IEnumerable<ISmartScriptLine> lines)
        {
            int? entry = null;
            SmartScriptType? source = null;
            int previousLink = -1;
            SmartEvent currentEvent = null;

            SortedDictionary<int, SmartEvent> triggerIdToActionParent = new();
            SortedDictionary<int, SmartEvent> triggerIdToEvent = new();
            
            foreach (var line in lines)
            {
                if (!entry.HasValue)
                    entry = line.EntryOrGuid;
                else
                    Debug.Assert(entry.Value == line.EntryOrGuid);

                if (!source.HasValue)
                    source = (SmartScriptType)line.ScriptSourceType;
                else
                    Debug.Assert((int)source.Value == line.ScriptSourceType);

                if (previousLink != line.Id)
                {
                    currentEvent = SafeEventFactory(line);
                    if (currentEvent != null)
                    {
                        if (currentEvent.Id == SmartConstants.EVENT_TRIGGER_TIMED)
                        {
                            triggerIdToEvent[currentEvent.GetParameter(0).Value] = currentEvent;
                        }
                        Events.Add(currentEvent);
                    }
                    else
                        continue;
                }

                var comment = line.Comment.Contains(" // ")
                    ? line.Comment.Substring(line.Comment.IndexOf(" // ") + 4).Trim()
                    : "";

                var action = SafeActionFactory(line);
                if (action != null)
                {
                    if (comment != SmartConstants.COMMENT_WAIT)
                        action.Comment = comment;
                    if (action.Id == SmartConstants.ACTION_TRIGGER_TIMED && comment == SmartConstants.COMMENT_WAIT)
                        triggerIdToActionParent[action.GetParameter(0).Value] = currentEvent;
                    currentEvent.AddAction(action);
                }
                previousLink = line.Link;
            }

            var sortedTriggers = triggerIdToEvent.Keys.ToList();
            sortedTriggers.Reverse();
            foreach (int triggerId in sortedTriggers)
            {
                var @event = triggerIdToEvent[triggerId];
                if (!triggerIdToActionParent.ContainsKey(triggerId))
                    continue;
                
                var caller = triggerIdToActionParent[triggerId];

                var lastAction = caller.Actions[caller.Actions.Count - 1];
                
                if (lastAction.Id != SmartConstants.ACTION_TRIGGER_TIMED || lastAction.GetParameter(1).Value != lastAction.GetParameter(2).Value)
                    continue;

                var waitTime = lastAction.GetParameter(1).Value;
                var waitAction =
                    smartFactory.ActionFactory(SmartConstants.ACTION_WAIT, smartFactory.SourceFactory(SmartConstants.SOURCE_NONE), smartFactory.TargetFactory(SmartConstants.TARGET_NONE));
                waitAction.SetParameter(0, waitTime);
                
                caller.Actions.RemoveAt(caller.Actions.Count - 1);
                caller.AddAction(waitAction);
                foreach (var a in @event.Actions)
                    caller.AddAction(a);
                Events.Remove(@event);
            }
        }

        public void InsertFromClipboard(int index, IEnumerable<ISmartScriptLine> lines)
        {
            SmartEvent currentEvent = null;
            int prevIndex = 0;
            foreach (var line in lines)
            {
                if (currentEvent == null || prevIndex != line.Id)
                {
                    prevIndex = line.Id;
                    currentEvent = SafeEventFactory(line);
                    Events.Insert(index++, currentEvent);
                }

                if (line.ActionType != -1)
                {
                    var action = SafeActionFactory(line);
                    if (action != null)
                        currentEvent.AddAction(action);                    
                }
            }
        }
        
        public SmartAction SafeActionFactory(ISmartScriptLine line)
        {
            try
            {
               return smartFactory.ActionFactory(line);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show($"Action {line.ActionType} unknown, skipping action");
            }
            return null;
        }

        public SmartEvent SafeEventFactory(ISmartScriptLine line)
        {
            try
            {
                return smartFactory.EventFactory(line);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show($"Event {line.EventType} unknown, skipping action");
            }
            return null;
        }
        
        public System.IDisposable BulkEdit(string name)
        {
            return new BulkEditing(this, name);
        }

        private class BulkEditing : System.IDisposable
        {
            private readonly SmartScript _smartScript;
            private readonly string _name;

            public BulkEditing(SmartScript smartScript, string name)
            {
                _smartScript = smartScript;
                _name = name;
                _smartScript.BulkEditingStarted.Invoke();
            }

            public void Dispose()
            {
                _smartScript.BulkEditingFinished.Invoke(_name);
            }
        }
    }

}
