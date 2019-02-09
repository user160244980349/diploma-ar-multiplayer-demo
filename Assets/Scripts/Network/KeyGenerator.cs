using System;

namespace Network
{
    public static class KeyGenerator
    {
        public static int Generate()
        {
            var dateTimeToConvert = DateTime.Now;

            var millisecsInASec = 1000;
            var millisecsInAMin = 60 * millisecsInASec;
            var millisecsInAnHour = 60 * millisecsInAMin;

            var key = dateTimeToConvert.Hour * millisecsInAnHour +
                           (dateTimeToConvert.Minute * millisecsInAMin) +
                           dateTimeToConvert.Second * millisecsInASec +
                           dateTimeToConvert.Millisecond;

            return key;
        }
    }
}
