using System;
using System.Collections.Generic;
using System.Linq;
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
        private const double ChanceOfInfection = 0.5;
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
            return Enumerable
                .Repeat(0, PeopleCount)
                .Select((_, index) => new Person(index, FindHome(), Map, index <= PeopleCount * 0.03))
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
            var healthInfectedPairs = 
                from healthy in healthyPeople
                from infected in infectedPeople
                select (healthy, infected);
            foreach (var (healthy, infected) in healthInfectedPairs)
            {
                if (_random.NextDouble() >= ChanceOfInfection &&
                    (healthy.State == PersonState.Walking && infected.State == PersonState.Walking && 
                    healthy.Position.GetDistanceTo(infected.Position) <= InfectionRadius || 
                     healthy.State == PersonState.AtHome && infected.State == PersonState.AtHome && 
                                                                  healthy.HomeId == infected.HomeId))
                    healthy.ChangeHealth(PersonHealth.Sick);
            }
        }
    }
}
