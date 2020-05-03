using System;
using System.Collections.Generic;
using System.Linq;
using covidSim.ClientApp.src.utils;
using covidSim.Utils;

namespace covidSim.Services
{
    public class Game
    {
        public List<Person> People;
        public CityMap Map;
        private DateTime _lastUpdate;

        private static Game _gameInstance;
        private static Random _random = new Random();
        
        public const int PeopleCount = 320;

        private const double InfectionRadius = 7.0;
        private const double HealingRadius = 7.0;
        private const double ChanceOfInfection = 0.5;
        private const double PercentageOfDoctors = 0.1;
        private const int DoctorCount = (int) (PeopleCount * PercentageOfDoctors);
        public const int FieldWidth = 1000;
        public const int FieldHeight = 500;
        public const int MaxPeopleInHouse = 10;

        private Game()
        {
            Map = new CityMap();
            People = CreatePopulation();
            _lastUpdate = DateTime.Now;
        }

        public static Game Instance => _gameInstance ?? (_gameInstance = new Game());

        public static void Restart()
        {
            _gameInstance = new Game();
        }

        private List<Person> CreatePopulation()
        {
            var doctorIndexes = Enumerable
                .Range(0, PeopleCount)
                .OrderBy(_ => _random.Next())
                .Take(DoctorCount)
                .ToHashSet();
            return Enumerable
                .Repeat(0, PeopleCount)
                .Select((_, index) => new Person(index, FindHome(), Map, GetHealth(doctorIndexes, index)))
                .ToList();
        }

        private int FindHome()
        {
            while (true)
            {
                var homeId = _random.Next(CityMap.HouseAmount);

                if (Map.Houses[homeId].ResidentCount < MaxPeopleInHouse)
                {
                    Map.Houses[homeId].ResidentCount++;
                    return homeId;
                }
            }
            
        }

        public Game GetNextState()
        {
            var diff = (DateTime.Now - _lastUpdate).TotalMilliseconds;
            if (diff >= 1000)
            {
                CalcNextStep();
                InfectPeople();
                HealPeople();
            }

            return this;
        }

        private void CalcNextStep()
        {
            _lastUpdate = DateTime.Now;
            var droppedOutPeople = new List<Person>();
            foreach (var person in People)
            {
                person.CalcNextStep();
                if (person.OutOfTheGame)
                    droppedOutPeople.Add(person);
            }
            droppedOutPeople.ForEach(p => People.Remove(p));
        }

        private void InfectPeople()
        {
            var infectedPeople = People.Where(p => p.Health == PersonHealth.Sick).ToList();
            var healthyPeople = People.Where(p => p.Health == PersonHealth.Healthy);
            var healthInfectedPairs = healthyPeople.Multiply(infectedPeople);
            foreach (var (healthy, infected) in healthInfectedPairs)
            {
                if (_random.NextDouble() >= ChanceOfInfection &&
                    (CanBeInfectedThroughWalk(healthy, infected) || CanBeInfectedThroughHome(healthy, infected)))
                    healthy.ChangeHealth(PersonHealth.Sick);
            }
        }

        private bool CanBeInfectedThroughWalk(Person healthy, Person infected)
        {
            return healthy.State == PersonState.Walking && infected.State == PersonState.Walking &&
                   healthy.Position.GetDistanceTo(infected.Position) <= InfectionRadius;
        }

        private bool CanBeInfectedThroughHome(Person healthy, Person infected)
        {
            return healthy.State == PersonState.AtHome && infected.State == PersonState.AtHome && 
                   healthy.HomeId == infected.HomeId;
        }

        private void HealPeople()
        {
            var doctors = People.Where(p => p.Health == PersonHealth.Doctor);
            var infectedPeople = People.Where(p => p.Health == PersonHealth.Sick);
            var pairs = doctors.Multiply(infectedPeople);
            foreach (var (doctor, infected) in pairs)
            {
                if (doctor.Position.GetDistanceTo(infected.Position) <= HealingRadius)
                    infected.ChangeHealth(PersonHealth.Healthy);
            }
        }
        
        private static PersonHealth GetHealth(ICollection<int> doctorIndexes, int index)
        {
            if (doctorIndexes.Contains(index))
                return PersonHealth.Doctor;
            if (index <= PeopleCount * 0.03)
                return PersonHealth.Sick;
            return PersonHealth.Healthy;
        }
    }
}
