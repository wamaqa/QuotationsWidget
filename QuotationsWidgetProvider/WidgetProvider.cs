using Microsoft.Windows.Widgets.Providers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage;

namespace QuotationsWidgetProvider
{

    public class CompactWidgetInfo
    {
        public string WidgetId { get; set; }
        public string WidgetName { get; set; }
        public int CustomState = 0;
        public bool IsActive = false;

    }

    internal class WidgetProvider : IWidgetProvider
    {
   

        public static Dictionary<string, CompactWidgetInfo> RunningWidgets = new Dictionary<string, CompactWidgetInfo>();

        public WidgetProvider()
        {
            var runningWidgets = WidgetManager.GetDefault().GetWidgetInfos();
            _dataService = new DataService();
            foreach (var widgetInfo in runningWidgets)
            {
                var widgetContext = widgetInfo.WidgetContext;
                var widgetId = widgetContext.Id;
                var widgetName = widgetContext.DefinitionId;
                var customState = widgetInfo.CustomState;
                if (!RunningWidgets.ContainsKey(widgetId))
                {
                    CompactWidgetInfo runningWidgetInfo = new CompactWidgetInfo() { WidgetId = widgetName, WidgetName = widgetId };
                    try
                    {
                        // If we had any save state (in this case we might have some state saved for Counting widget)
                        // convert string to required type if needed.
                        int count = Convert.ToInt32(customState.ToString());
                        runningWidgetInfo.CustomState = count;
                    }
                    catch
                    {

                    }
                    RunningWidgets[widgetId] = runningWidgetInfo;
                }
            }
        }


        static ManualResetEvent emptyWidgetListEvent = new ManualResetEvent(false);
        private DataService _dataService;
        private Task _curTask;
        private bool activeAuto;

        public static ManualResetEvent GetEmptyWidgetListEvent()
        {
            return emptyWidgetListEvent;
        }

        public void Activate(WidgetContext widgetContext)
        {
            var widgetId = widgetContext.Id;

            if (RunningWidgets.ContainsKey(widgetId))
            {
                var localWidgetInfo = RunningWidgets[widgetId];
                localWidgetInfo.IsActive = true;
                UpdateWidget(localWidgetInfo);
                AutoRefresh(true);
            }
        }

        void Refresh()
        {
            while (activeAuto)
            {
                Task.Delay(1000);
                RunningWidgets.Values.ToList().ForEach(item =>
                {
                    UpdateWidget(item);
                });
            }
        }

        private void AutoRefresh(bool active)
        {
            activeAuto = active;
            if(_curTask==null)
            {
                _curTask = new Task(Refresh);
            }
            if(active && _curTask.Status != TaskStatus.Running)
                _curTask.Start();
        }

        public void Deactivate(string widgetId)
        {
            if (RunningWidgets.ContainsKey(widgetId))
            {
                var localWidgetInfo = RunningWidgets[widgetId];
                localWidgetInfo.IsActive = false;
                AutoRefresh(false);
            }
        }

        public void CreateWidget(WidgetContext widgetContext)
        {
            var widgetId = widgetContext.Id; // To save RPC calls
            var widgetName = widgetContext.DefinitionId;
            CompactWidgetInfo runningWidgetInfo = new CompactWidgetInfo() { WidgetId = widgetId, WidgetName = widgetName };
            RunningWidgets[widgetId] = runningWidgetInfo;
            UpdateWidget(runningWidgetInfo);
        }

        private void UpdateWidget(CompactWidgetInfo localWidgetInfo)
        {
            WidgetUpdateRequestOptions updateOptions = new WidgetUpdateRequestOptions(localWidgetInfo.WidgetId);
            updateOptions.Template = _dataService.Layout;
            updateOptions.Data = _dataService.RequsetQuotation();
            updateOptions.CustomState = localWidgetInfo.CustomState.ToString();
            //var s = WidgetManager.GetDefault().GetWidgetInfos();
            //var ss = JsonConvert.SerializeObject(s);
            File.WriteAllText("C:\\ProgramData\\QuotationsWidget\\a.json", _dataService.Layout);
            File.WriteAllText("C:\\ProgramData\\QuotationsWidget\\b.json", updateOptions.Data);

            WidgetManager.GetDefault().UpdateWidget(updateOptions);
        }



        public void DeleteWidget(string widgetId, string customState)
        {
            if(RunningWidgets.ContainsKey(widgetId))
            {
                RunningWidgets.Remove(widgetId);

                if (RunningWidgets.Count == 0)
                {
                    emptyWidgetListEvent.Set();
                }
            }
        }

        public void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
        {
            var verb = actionInvokedArgs.Verb;
            if (verb == "inc")
            {
                var widgetId = actionInvokedArgs.WidgetContext.Id;
                // If you need to use some data that was passed in after
                // Action was invoked, you can get it from the args:
                var data = actionInvokedArgs.Data;
                if (RunningWidgets.ContainsKey(widgetId))
                {
                    var localWidgetInfo = RunningWidgets[widgetId];
                    // Increment the count
                    localWidgetInfo.CustomState++;
                    UpdateWidget(localWidgetInfo);
                }
            }
        }

        public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
        {
            var widgetContext = contextChangedArgs.WidgetContext;
            var widgetId = widgetContext.Id;
            var widgetSize = widgetContext.Size;
            if (RunningWidgets.ContainsKey(widgetId))
            {
                var localWidgetInfo = RunningWidgets[widgetId];
                UpdateWidget(localWidgetInfo);
            }
        }
    }
}
