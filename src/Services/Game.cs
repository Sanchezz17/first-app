using System;
using System.Collections.Generic;
using System.Linq;
using covidSim.ClientApp.src.utils;
using covidSim.Models;
using covidSim.Utils;

namespace covidSim.Services
{
    public class Game
    {
        public GameConfiguration Configuration { get; set; }
        public List<Person> People;
        public CityMap Map;
        private DateTime _lastUpdate;

        private static Game _gameInstance;
        private static Random _random = new Random();
        
        public const int PeopleCount = 320;

        private const double InfectionRadius = 7.0;
        private const double HealingRadius = 7.0;
        private const double ChanceOfInfection = 0.5;
        public const int FieldWidth = 1000;
        public const int FieldHeight = 500;

        private Game()
        {
            Configuration = new GameConfiguration();
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
                .Take((int) (PeopleCount * Configuration.PercentageOfDoctors))
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

                if (Map.Houses[homeId].ResidentCount < Configuration.MaxPeopleInHouse)
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
            var walkingPeople = People.Where(p => p.State == PersonState.Walking).ToList();
            var infectedPeople = walkingPeople.Where(p => p.Health == PersonHealth.Sick).ToList();
            var healthyPeople = walkingPeople.Where(p => p.Health == PersonHealth.Healthy);
            var pairs = healthyPeople.Multiply(infectedPeople);
            foreach (var (healthy, infected) in pairs)
            {
                if (healthy.Position.GetDistanceTo(infected.Position) <= InfectionRadius && 
                    _random.NextDouble() >= ChanceOfInfection)
                    healthy.ChangeHealth(PersonHealth.Sick);
            }
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
