namespace covidSim.Models
{
    public class GameConfiguration
    {
        public double PercentageOfDoctors { get; set; } = 0.1;
        public double PercentageOfInfectedAtMeeting { get; set; } = 0.03;
        public double ChanceOfInfectionAtHome { get; set; } = 0.5;
        public double ChanceOfBeingCuredByDoctor { get; set; } = 1.0;
        public int RecoveryTime { get; set; } = 35;
        public double ProbabilityOfDying { get; set; } = 0.000003;
        public double ProbabilityToLeaveHome { get; set; } = 0.005;
        public int MaxPeopleInHouse { get; set; } = 10;
    }
}