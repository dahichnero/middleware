namespace DITwo
{
    public class EngDayOfWeek :IDayOfWeek
    {
        public string GetDayOfWeek () => Enum.GetName<DayOfWeek>(DateTime.Today.DayOfWeek);
    }
}
