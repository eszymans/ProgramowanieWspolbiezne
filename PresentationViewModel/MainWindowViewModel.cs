//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        #region ctor

        public MainWindowViewModel() : this(null)
        { }

        internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
            StartCommand = new RelayCommand(StartSimulation, () => NumberOfBalls > 0);
            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
        }

        #endregion ctor

        #region public API

        public void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));

            ModelLayer.Start(numberOfBalls);
            Observer.Dispose();
        }

        private int numberOfBalls;
        public int NumberOfBalls
        {
            get => numberOfBalls;
            set
            {
                numberOfBalls = value;
                RaisePropertyChanged(nameof(NumberOfBalls));
                (StartCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand StartCommand { get; }

        private void StartSimulation()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));

            // Wywołanie metody zapytania o liczbę piłek w View
            AskForNumberOfBalls();
        }

        public void AskForNumberOfBalls()
        {
            // Przykładowo, wywołanie okna dialogowego
            var input = Microsoft.VisualBasic.Interaction.InputBox(
              "Please enter the number of balls:",
              "Number of Balls",
              "10");

            if (int.TryParse(input, out int result))
            {
                NumberOfBalls = result;
            }
            else
            {
                // Obsługa niepoprawnego wpisu (np. pusta wartość lub tekst)
                Console.WriteLine("Invalid input. Please enter a valid number.");

            }
        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        #endregion public API

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Balls.Clear();
                    Observer.Dispose();
                    ModelLayer.Dispose();
                }

                Disposed = true;
            }
        }

        public void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private IDisposable Observer = null;
        private ModelAbstractApi ModelLayer;
        private bool Disposed = false;

        #endregion private
    }


}