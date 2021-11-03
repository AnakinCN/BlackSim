using System;

namespace OrderDeliver
{
    class Program
    {
        static void Main(string[] args)
        {
            //default parameters
            bool firstin = false;
            double rate = 1.0f;

            //parsing parameters from args
            foreach(string arg in args)
            {
                if (arg == "firstin")
                    firstin = true;

                double rateParsed = 1.0f;
                if (double.TryParse(arg, out rateParsed))
                    rate = rateParsed;
            }

            Story story = new Story(firstIn: firstin, accelRate: rate);
            story.Run();
        }
    }
}
