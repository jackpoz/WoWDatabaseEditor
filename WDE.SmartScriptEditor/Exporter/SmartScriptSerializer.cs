﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using WDE.Common.Database;
using WDE.SmartScriptEditor.Data;
using WDE.SmartScriptEditor.Models;

namespace WDE.SmartScriptEditor.Exporter
{
    public static class SmartScriptSerializer
    {
        private static readonly Regex SAI_LINE_REGEX = new Regex(
            @"\(\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(\d+(?:\.\d+)?),\s*(\d+(?:\.\d+)?),\s*(\d+(?:\.\d+)?),\s*(\d+(?:\.\d+)?),\s*"".*?""\s*\)"); 
        private static readonly string SAI_SQL = "({entryorguid}, {source_type}, {id}, {linkto}, {event_id}, {phasemask}, {chance}, {flags}, {event_param1}, {event_param2}, {event_param3}, {event_param4}, {action_id}, {action_param1}, {action_param2}, {action_param3}, {action_param4}, {action_param5}, {action_param6}, {target_id}, {target_param1}, {target_param2}, {target_param3}, {x}, {y}, {z}, {o}, \"{comment}\")";

        public static bool TryToISmartScriptLine(this string str, out ISmartScriptLine line)
        {
            line = new AbstractSmartScriptLine();

            Match m = SAI_LINE_REGEX.Match(str);
            if (!m.Success || m.Groups.Count != 28)
                return false;

            line.EntryOrGuid = int.Parse(m.Groups[1].ToString());
            line.ScriptSourceType = int.Parse(m.Groups[2].ToString());
            line.Id = int.Parse(m.Groups[3].ToString());
            line.Link = int.Parse(m.Groups[4].ToString());
            line.EventType = int.Parse(m.Groups[5].ToString());
            line.EventPhaseMask = int.Parse(m.Groups[6].ToString());
            line.EventChance = int.Parse(m.Groups[7].ToString());
            line.EventFlags = int.Parse(m.Groups[8].ToString());
            line.EventParam1 = int.Parse(m.Groups[9].ToString());
            line.EventParam2 = int.Parse(m.Groups[10].ToString());
            line.EventParam3 = int.Parse(m.Groups[11].ToString());
            line.EventParam4 = int.Parse(m.Groups[12].ToString());
            line.ActionType = int.Parse(m.Groups[13].ToString());
            line.ActionParam1 = int.Parse(m.Groups[14].ToString());
            line.ActionParam2 = int.Parse(m.Groups[15].ToString());
            line.ActionParam3 = int.Parse(m.Groups[16].ToString());
            line.ActionParam4 = int.Parse(m.Groups[17].ToString());
            line.ActionParam5 = int.Parse(m.Groups[18].ToString());
            line.ActionParam6 = int.Parse(m.Groups[19].ToString());
            line.TargetType = int.Parse(m.Groups[20].ToString());
            line.TargetParam1 = int.Parse(m.Groups[21].ToString());
            line.TargetParam2 = int.Parse(m.Groups[22].ToString());
            line.TargetParam3 = int.Parse(m.Groups[23].ToString());
            line.TargetX = float.Parse(m.Groups[24].ToString(), CultureInfo.InvariantCulture);
            line.TargetY = float.Parse(m.Groups[25].ToString(), CultureInfo.InvariantCulture);
            line.TargetZ = float.Parse(m.Groups[26].ToString(), CultureInfo.InvariantCulture);
            line.TargetO = float.Parse(m.Groups[27].ToString(), CultureInfo.InvariantCulture);
            line.Comment = m.Groups[28].ToString();
            
            return true;
        }
        
        public static string ToSqlString(this ISmartScriptLine line)
        {
            
            object data = new
            {
                entryorguid = line.EntryOrGuid.ToString(),
                source_type = line.SourceType.ToString(),
                id = line.Id.ToString(),
                linkto = line.Link.ToString(),

                event_id = line.EventType.ToString(),
                phasemask = line.EventPhaseMask.ToString(),
                chance = line.EventChance.ToString(),
                flags = line.EventFlags.ToString(),
                event_param1 = line.EventParam1.ToString(),
                event_param2 = line.EventParam2.ToString(),
                event_param3 = line.EventParam3.ToString(),
                event_param4 = line.EventParam4.ToString(),

                event_cooldown_min = line.EventCooldownMin.ToString(),
                event_cooldown_max = line.EventCooldownMax.ToString(),

                action_id = line.ActionType.ToString(),
                action_param1 = line.ActionParam1.ToString(),
                action_param2 = line.ActionParam2.ToString(),
                action_param3 = line.ActionParam3.ToString(),
                action_param4 = line.ActionParam4.ToString(),
                action_param5 = line.ActionParam5.ToString(),
                action_param6 = line.ActionParam6.ToString(),

                action_source_id = line.SourceType.ToString(),
                source_param1 = line.SourceParam1.ToString(),
                source_param2 = line.SourceParam2.ToString(),
                source_param3 = line.SourceParam3.ToString(),
                source_condition_id = line.SourceConditionId.ToString(),

                target_id = line.TargetType.ToString(),
                target_param1 = line.TargetParam1.ToString(),
                target_param2 = line.TargetParam2.ToString(),
                target_param3 = line.TargetParam3.ToString(),
                target_condition_id = line.TargetConditionId.ToString(),

                x = line.TargetX.ToString(CultureInfo.InvariantCulture),
                y = line.TargetY.ToString(CultureInfo.InvariantCulture),
                z = line.TargetZ.ToString(CultureInfo.InvariantCulture),
                o = line.TargetO.ToString(CultureInfo.InvariantCulture),

                comment = line.Comment
            };

            return SmartFormat.Smart.Format(SAI_SQL, data);
        }

        public static ISmartScriptLine[] ToWaitFreeSmartScriptLines(this SmartScript script, ISmartFactory smartFactory)
        {
            if (script.Events.Count == 0)
                return new ISmartScriptLine[0];

            int eventId = 0;
            List<ISmartScriptLine> lines = new List<ISmartScriptLine>();
            bool previousWasWait = false;
            int nextTriggerId = script.Events.Where(e => e.Id == SmartConstants.EVENT_TRIGGER_TIMED).Select(e => e.GetParameter(0).Value).DefaultIfEmpty(0).Max() + 1;
            
            //@todo: don't use hardcoded IDs!!!!
            foreach (SmartEvent e in script.Events)
            {
                if (e.Actions.Count == 0)
                    continue;

                e.ActualId = eventId;

                for (int index = 0; index < e.Actions.Count; ++index)
                {
                    SmartEvent actualEvent = e;

                    if (previousWasWait)
                    {
                        actualEvent = smartFactory.EventFactory(SmartConstants.EVENT_TRIGGER_TIMED);
                        actualEvent.SetParameter(0, nextTriggerId++);
                    }
                    else if (index > 0)
                        actualEvent = smartFactory.EventFactory(SmartConstants.EVENT_LINK);

                    int linkTo = (e.Actions.Count - 1 == index ? 0 : eventId + 1);

                    SmartAction actualAction = e.Actions[index];

                    if (actualAction.Id == SmartConstants.ACTION_WAIT)
                    {
                        linkTo = 0;
                        SmartAction waitAction = actualAction;
                        actualAction = smartFactory.ActionFactory(SmartConstants.ACTION_TRIGGER_TIMED, smartFactory.SourceFactory(SmartConstants.SOURCE_NONE), smartFactory.TargetFactory(SmartConstants.TARGET_NONE));
                        actualAction.SetParameter(0, nextTriggerId);
                        actualAction.SetParameter(1, waitAction.GetParameter(0).GetValue());
                        actualAction.SetParameter(2, waitAction.GetParameter(0).GetValue());
                        actualAction.Comment = SmartConstants.COMMENT_WAIT;
                        previousWasWait = true;
                    }
                    else
                        previousWasWait = false;

                    SmartEvent eventToSerialize = actualEvent.ShallowCopy();
                    eventToSerialize.Actions.Add(actualAction.Copy());

                    var serialized =
                        eventToSerialize.ToSmartScriptLines(script.EntryOrGuid, script.SourceType, eventId, linkTo);
                    
                    if (serialized.Length != 1)
                        throw new InvalidOperationException();
                    
                    lines.Add(serialized[0]);

                    eventId++;
                }
            }

            return lines.ToArray();
        }

        public static ISmartScriptLine[] ToSmartScriptLines(this SmartEvent e, int scriptEntryOrGuid, SmartScriptType scriptSourceType, int id, int? linkTo = null)
        {
            List<ISmartScriptLine> lines = new List<ISmartScriptLine>();
            IEnumerable<SmartAction> actions = e.Actions.Count == 0 ? new List<SmartAction>()
            {
                new SmartAction(-1, new SmartSource(-1){ReadableHint = ""}, new SmartTarget(-1){ReadableHint = ""})
                {
                    ReadableHint = ""
                }
            } : e.Actions;
            
            foreach (var a in actions)
            {
                var line = new AbstractSmartScriptLine()
                {
                    EntryOrGuid = scriptEntryOrGuid,
                    ScriptSourceType = (int)scriptSourceType,
                    Id = id,
                    Link = linkTo ?? 0,
                    EventType = e.Id,
                    EventPhaseMask = e.Phases.Value,
                    EventChance = e.Chance.Value,
                    EventFlags = e.Flags.Value,
                    EventParam1 = e.GetParameter(0).Value,
                    EventParam2 = e.GetParameter(1).Value,
                    EventParam3 = e.GetParameter(2).Value,
                    EventParam4 = e.GetParameter(3).Value,
                    EventCooldownMin = e.CooldownMin.Value,
                    EventCooldownMax = e.CooldownMax.Value,
                    ActionType = a.Id,
                    ActionParam1 = a.GetParameter(0).Value,
                    ActionParam2 = a.GetParameter(1).Value,
                    ActionParam3 = a.GetParameter(2).Value,
                    ActionParam4 = a.GetParameter(3).Value,
                    ActionParam5 = a.GetParameter(4).Value,
                    ActionParam6 = a.GetParameter(5).Value,
                    SourceType = a.Source.Id,
                    SourceParam1 = a.Source.GetParameter(0).Value,
                    SourceParam2 = a.Source.GetParameter(1).Value,
                    SourceParam3 = a.Source.GetParameter(2).Value,
                    SourceConditionId = 0,
                    TargetType = a.Target.Id,
                    TargetParam1 = a.Target.GetParameter(0).Value,
                    TargetParam2 = a.Target.GetParameter(1).Value,
                    TargetParam3 = a.Target.GetParameter(2).Value,
                    TargetConditionId = 0,
                    TargetX = a.Target.X,
                    TargetY = a.Target.Y,
                    TargetZ = a.Target.Z,
                    TargetO = a.Target.O,
                    Comment = e.Readable + " - " + a.Readable + (string.IsNullOrEmpty(a.Comment) ? "" : (" // " + a.Comment)),
                };   
                lines.Add(line);
            }

            return lines.ToArray();
        }
    }
}