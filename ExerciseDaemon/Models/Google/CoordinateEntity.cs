namespace ExerciseDaemon.Models.Google
{
    public struct CoordinateEntity
    {
        public double Latitude;
        public double Longitude;

        public CoordinateEntity(double x, double y)
        {
            Latitude = x; Longitude = y;
        }
    }
}