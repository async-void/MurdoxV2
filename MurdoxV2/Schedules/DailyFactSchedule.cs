using MurdoxV2.Services;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Schedules
{
    public class DailyFactSchedule(IScheduler scheduler)
    {
        public async Task ScheduleAsync()
        {
            var jobKey = new JobKey("dailyFactJob", "facts");

            if (await scheduler.CheckExists(jobKey))
                return;

            var trigger = TriggerBuilder.Create()
                .WithIdentity("dailyFactTrigger", "facts")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(trigger); // ✅ Now you're passing IJobDetail
        }
    }
}
