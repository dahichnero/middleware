namespace DITwo
{
    public class AugustDayGetter :ISpecialDateGetter
    {
        public string GetSpecialDate()
        {
            DateTime dateTime = DateTime.Today;
            double august;
            if (dateTime.Month < 8)
            {
                august = (dateTime -new DateTime(dateTime.Year - 1, 8, 1)).TotalDays;
            } 
            else
            {
                august = (dateTime - new DateTime(dateTime.Year, 8, 1)).TotalDays;
            }
            return $"{august} августа";
        }
    }
}
