using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;

namespace NFCRing.UI.ViewModel
{
    public class WizardViewModel : ContentViewModel
    {
        private readonly List<IStepViewModel> _steps;

        private IStepViewModel _stepViewModel;
        private bool _isBusy;

        public IStepViewModel StepViewModel
        {
            get { return _stepViewModel; }
            set { Set(ref _stepViewModel, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        public string NextText => StepViewModel?.NextText;

        public Action CancelAction { get; set; }

        public RelayCommand CancelCommand { get; }

        public RelayCommand NextCommand { get; }

        public WizardViewModel(List<IStepViewModel> steps)
        {
            Title = "NFC Ring Login Setup";

            _steps = steps;

            InitializeSteps(_steps);

            StepViewModel = _steps.FirstOrDefault();

            CancelCommand = new RelayCommand(Cancel);
            NextCommand = new RelayCommand(Next);

            PropertyChanged += OnPropertyChanged;
        }

        private void InitializeSteps(List<IStepViewModel> steps)
        {
            if (steps == null)
                return;

            var newRingViewModel = new NewRingViewModel();

            foreach (var stepViewModel in steps)
            {
                SetNewRing(stepViewModel, newRingViewModel);

                if (stepViewModel.ToNext != null)
                    stepViewModel.ToNext += ToNext;
            }
        }

        private static void SetNewRing(IStepViewModel stepViewModel, NewRingViewModel newRingViewModel)
        {
            stepViewModel.NewRingViewModel = newRingViewModel;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StepViewModel))
                RaisePropertyChanged(() => NextText);
            if (e.PropertyName == nameof(IsBusy))
                ServiceLocator.Current.GetInstance<MainViewModel>().IsBusy = IsBusy;
        }

        private async void Next()
        {
            if (StepViewModel == null)
                return;

            if (StepViewModel.NextAction != null)
            {
                IsBusy = true;

                try
                {
                    var useDefaultBehavior = await StepViewModel.NextAction();
                    if (!useDefaultBehavior)
                        return;
                }
                finally
                {
                    IsBusy = false;
                }
            }

            ToNext();
        }

        private void ToNext()
        {
            var currentStepIndex = _steps.IndexOf(StepViewModel);
            if (currentStepIndex < 0)
                return;

            if (currentStepIndex == _steps.Count - 1)
                return;

            StepViewModel = _steps[currentStepIndex + 1];
        }

        private void Cancel()
        {
            StepViewModel.CancelAction?.Invoke();
            CancelAction?.Invoke();
        }
    }
}
