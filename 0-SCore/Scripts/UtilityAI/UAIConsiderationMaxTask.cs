using System.Collections.Generic;
using System.Globalization;
using Harmony.UtilityAI;

namespace UAI
{

    public class UAIConsiderationMaxTask : UAIConsiderationBase
    {
        private int _maxCount = 5;
        private string _tasks;
        public override void Init(Dictionary<string, string> _parameters)
        {
            base.Init(_parameters);
            if (_parameters.ContainsKey("maxCount"))
               _maxCount = StringParsers.ParseSInt16(_parameters["maxCount"], 0, -1, NumberStyles.Any);
            if (_parameters.ContainsKey("task"))
                _tasks = _parameters["task"].ToString();
        }

        public override float GetScore(Context context, object target)
        {
            if (!UAITaskBasePatches.TaskHistory.ContainsKey(context.Self.entityId)) return 1f;
            var taskHistory = UAITaskBasePatches.TaskHistory[context.Self.entityId];
            var counter = 0;
            foreach (var item in taskHistory)
            {
                if (_tasks.Contains(item.ToString()))
                    counter++;
            }

            return counter == _maxCount ? 0f : 1f;
        }
    }
}