using System;
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace LocalLlmClient.Tools
{
    public class TimeTool
    {
        [KernelFunction("get_current_time")]
        [Description("Gets the current local date and time. Useful when you need to answer questions about 'now' or 'today'.")]
        public string GetCurrentTime()
        {
            return DateTime.Now.ToString("F");
        }
        
        [KernelFunction("get_current_year")]
        [Description("Gets the current year.")]
        public int GetCurrentYear()
        {
            return DateTime.Now.Year;
        }
    }
}
