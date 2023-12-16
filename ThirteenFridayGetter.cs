namespace DITwo
{
    public class ThirteenFridayGetter :ISpecialDateGetter
    {
        public string GetSpecialDate()
        {
            DateTime dateTime = DateTime.Today;
            int indDate = (int) dateTime.DayOfWeek;
            if (indDate < 5)
            {
                dateTime=dateTime.AddDays(5 - indDate);
            }
            if (indDate > 5)
            {
                dateTime=dateTime.AddDays(-(indDate-5));
            }
            while (true)
            {
                if (dateTime.Day == 13)
                {
                    return dateTime.ToString();
                } 
                else
                {
                    dateTime=dateTime.AddDays(7);
                }
            }
        }
    }
}
