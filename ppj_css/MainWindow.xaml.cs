using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EpidemicSpreadSimulator
{
    public partial class MainWindow : Window
    {
        // Lista populacije (osoba u simulaciji)
        private List<Person> population = new List<Person>();

        // Generiranje slučajnih brojeva
        private Random random = new Random();

        // Timer za simulaciju
        private DispatcherTimer timer = new DispatcherTimer();



        //Postavljanje parametara simulacije
        private int populationSize = 200; 
        private int initialInfected = 5;  
        private double transmissionRate = 0.5; 
        private double recoveryRate = 0.01;    

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => InitializePopulation();

            // Postavljanje intervala i dodavanje događaja na timer
            timer.Interval = TimeSpan.FromMilliseconds(20);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void InitializePopulation()
        {
            for (int i = 0; i < populationSize; i++)
            {
                Person person = new Person
                {
                    Status = i < initialInfected ? InfectionStatus.Infected : InfectionStatus.Healthy,
                    Position = new Point(random.NextDouble() * SimulationCanvas.ActualWidth, random.NextDouble() * SimulationCanvas.ActualHeight),
                    Velocity = new Point((random.NextDouble() * 6 - 3), (random.NextDouble() * 6 - 3)) 
                };

                Ellipse ellipse = new Ellipse
                {
                    // Veličina točkica
                    Width = 10,
                    Height = 10,

                    Fill = person.Status == InfectionStatus.Infected ? Brushes.Red : Brushes.Green
                };

                person.Shape = ellipse;
                SimulationCanvas.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, person.Position.X);
                Canvas.SetTop(ellipse, person.Position.Y);
                population.Add(person);
            }
        }


        private void Timer_Tick(object sender, EventArgs e)
        {

            foreach (var person in population)
            {
                person.Move(SimulationCanvas.ActualWidth, SimulationCanvas.ActualHeight);
                person.CheckInfection(population, transmissionRate, random);
                person.Recover(recoveryRate, random);

                Canvas.SetLeft(person.Shape, person.Position.X);
                Canvas.SetTop(person.Shape, person.Position.Y);

                if (person.Status == InfectionStatus.Infected)
                {
                    person.Shape.Fill = Brushes.Red;
                }
                else if (person.Status == InfectionStatus.Immune)
                {
                    person.Shape.Fill = Brushes.Blue;
                }
                else
                {
                    person.Shape.Fill = Brushes.Green;
                }
            }

        }
    }

    public class Person
    {
        public InfectionStatus Status { get; set; }
        public Point Position { get; set; }
        public Point Velocity { get; set; }
        public Ellipse Shape { get; set; }
        private int timeSinceInfection = 0;

        // Metoda za kretanje osobe i odbijanje od rubova platna
        public void Move(double canvasWidth, double canvasHeight)
        {
            Position = new Point(Position.X + Velocity.X, Position.Y + Velocity.Y);

            if (Position.X < 0 || Position.X > canvasWidth)
                Velocity = new Point(-Velocity.X, Velocity.Y);
            if (Position.Y < 0 || Position.Y > canvasHeight)
                Velocity = new Point(Velocity.X, -Velocity.Y);
        }

        // Metoda za provjeru zaraze s drugim osobama u populaciji
        public void CheckInfection(List<Person> population, double transmissionRate, Random random)
        {
            if (Status != InfectionStatus.Infected) return;

            foreach (var other in population)
            {
                if (other.Status == InfectionStatus.Healthy && IsCloseTo(other))
                {
                    if (random.NextDouble() < transmissionRate)
                    {
                        other.Status = InfectionStatus.Infected;
                    }
                }
            }
        }

        // Metoda za ozdravljenje od zaraze
        public void Recover(double recoveryRate, Random random)
        {
            if (Status == InfectionStatus.Infected)
            {
                timeSinceInfection++;

                if (timeSinceInfection > 100 && random.NextDouble() < recoveryRate)
                {
                    Status = InfectionStatus.Immune;
                    timeSinceInfection = 0; 
                }
            }
        }

        // Metoda za provjeru udaljenosti od druge osobe
        private bool IsCloseTo(Person other)
        {
            double distanceThreshold = 10.0;
            double dx = Position.X - other.Position.X;
            double dy = Position.Y - other.Position.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            return distance < distanceThreshold;
        }
    }

    // Enumeracija za status zaraze
    public enum InfectionStatus
    {
        Healthy,
        Infected,
        Immune
    }
}
