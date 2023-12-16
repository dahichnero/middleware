using System.Reflection.Metadata.Ecma335;

namespace DITwo
{
    public class RusDayfWeek :IDayOfWeek
    {
        
        public string GetDayOfWeek ()
        {
            string[] names = new string[]
            {
            "воскресенье", "понедельник", "вторник",
            "среда", "четверг", "пятница", "суббота"
            };
            int dayIndex = (int) DateTime.Today.DayOfWeek;
            return names[dayIndex];
        }
    }
}
