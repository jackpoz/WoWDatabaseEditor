﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WDE.Common.Database;
using WDE.SmartScriptEditor.Data;
using WDE.SmartScriptEditor.Models;
using Prism.Ioc;

namespace WDE.SmartScriptEditor.Exporter
{
    public class SmartScriptExporter
    {
        private static readonly string SAI_SQL = "({entryorguid}, {source_type}, {id}, {linkto}, {event_id}, {phasemask}, {chance}, {flags}, {event_param1}, {event_param2}, {event_param3}, {event_param4}, {action_id}, {action_param1}, {action_param2}, {action_param3}, {action_param4}, {action_param5}, {action_param6}, {target_id}, {target_param1}, {target_param2}, {target_param3}, {x}, {y}, {z}, {o}, \"{comment}\")";

        private readonly SmartScript _script;
        private readonly ISmartFactory smartFactory;
        private StringBuilder _sql = new StringBuilder();

        public SmartScriptExporter(SmartScript script, ISmartFactory smartFactory)
        {
            _script = script;
            this.smartFactory = smartFactory;
        }

        public string GetSql()
        {
            BuildHeader();
            return _sql.ToString();
        }

        private void BuildHeader()
        {
            _sql.AppendLine($"SET @ENTRY := {_script.EntryOrGuid};");
            BuildDelete();
            BuildUpdate();
            BuildInsert();
        }

        private void BuildInsert()
        {
            if (_script.Events.Count == 0)
                return;

            _sql.AppendLine(
                "INSERT INTO smart_scripts (entryorguid, source_type, id, link, event_type, event_phase_mask, event_chance, event_flags, event_param1, event_param2, event_param3, event_param4, action_type, action_param1, action_param2, action_param3, action_param4, action_param5, action_param6, target_type, target_param1, target_param2, target_param3, target_x, target_y, target_z, target_o, comment) VALUES");

            var serializedScript = _script.ToWaitFreeSmartScriptLines(smartFactory);
            IEnumerable<string> lines = serializedScript.Select(GenerateSingleSai);
            
            _sql.Append(string.Join(",\n", lines));
            _sql.AppendLine(";");
        }

        private string GenerateSingleSai(ISmartScriptLine line)
        {
            //if (action.Id == 188) // ACTION DEBUG MESSAGE
            //    comment = action.Comment;
            object data = new
            {
                entryorguid = "@ENTRY",
                source_type = ((int)_script.SourceType).ToString(),
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

        private void BuildUpdate()
        {
            switch (_script.SourceType)
            {
                case SmartScriptType.Creature:
                    _sql.AppendLine(
                        "UPDATE creature_template SET AIName=\"SmartAI\" WHERE entry= @ENTRY;");
                    break;
                case SmartScriptType.GameObject:
                    _sql.AppendLine(
                        "UPDATE gameobject_template SET AIName=\"SmartGameObjectAI\" WHERE entry=@ENTRY;");
                    break;
                case SmartScriptType.Quest:
                    _sql.AppendLine("DELETE FROM quest_scripts WHERE questId = @ENTRY;");
                    _sql.AppendLine("INSERT INTO quest_scripts(questId, ScriptName) VALUES(@ENTRY, \"SmartQuest\");");
                    break;
                case SmartScriptType.Spell:
                    _sql.AppendLine("DELETE FROM spell_script_names WHERE spell_id = @ENTRY And ScriptName=\"SmartSpell\";");
                    _sql.AppendLine("INSERT INTO spell_script_names(spell_id, ScriptName) VALUES(@ENTRY, \"SmartSpell\");");
                    break;
                case SmartScriptType.Aura:
                    _sql.AppendLine("DELETE FROM spell_script_names WHERE spell_id = @ENTRY And ScriptName=\"SmartAura\";");
                    _sql.AppendLine("INSERT INTO spell_script_names(spell_id, ScriptName) VALUES(@ENTRY, \"SmartAura\");");
                    break;
                case SmartScriptType.Cinematic:
                    _sql.AppendLine("DELETE FROM cinematic_scripts WHERE cinematicId = @ENTRY;");
                    _sql.AppendLine("INSERT INTO cinematic_scripts(cinematicId, ScriptName) VALUES(@ENTRY, \"SmartCinematic\");");
                    break;
                case SmartScriptType.AreaTrigger:
                    _sql.AppendLine("DELETE FROM areatrigger_scripts WHERE entry = @ENTRY;");
                    _sql.AppendLine("INSERT INTO areatrigger_scripts(entry, ScriptName) VALUES(@ENTRY, \"SmartTrigger\");");
                    break;
            }
        }

        private void BuildDelete()
        {
            _sql.AppendLine(
                $"DELETE FROM smart_scripts WHERE entryOrGuid = {_script.EntryOrGuid} AND source_type = {(int)_script.SourceType};");
        }
    }
}
