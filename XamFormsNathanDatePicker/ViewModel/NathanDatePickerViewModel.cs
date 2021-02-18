using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XamFormsNathanDatePicker
{
    public class NathanDatePickerViewModel : INotifyPropertyChanged
    {
        private readonly NathanDatePickerView _calendarView;

        public Command<Direction> PreviousCalendarCommand { get; }
        public Command<Direction> NextCalendarCommand { get; }
        public ICommand CurrentCalendarCommand { get; set; }

        private string _currentDate;
        public string CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                RaisePropertyChanged();
                PreviousCalendarCommand.ChangeCanExecute();
                NextCalendarCommand.ChangeCanExecute();
            }
        }

        private ViewType _currentCalendarView;
        public ViewType CurrentCalendarView
        {
            get => _currentCalendarView;
            set
            {
                _currentCalendarView = value;
                RaisePropertyChanged();
            }
        }

        public NathanDatePickerViewModel(NathanDatePickerView calendarView)
        {
            this._calendarView = calendarView;
            _currentCalendarView = ViewType.MonthView;

            CurrentCalendarCommand = new Command(UpdateCalendarCurrent);
            PreviousCalendarCommand = new Command<Direction>(UpdateCalendarWithDirection, CanExecute);
            NextCalendarCommand = new Command<Direction>(UpdateCalendarWithDirection, CanExecute);
        }

        private bool CanExecute(Direction direction)
        {
            var currentShowingYear = _calendarView._currentShowingDateTime.Year;
            var currrentShowingMonth = _calendarView._currentShowingDateTime.Month;
            var currentShowingDateTimeDaysInMonth = DateTime.DaysInMonth(currentShowingYear, currrentShowingMonth);

            int minDay = _calendarView.MinDateRange.Day;
            int maxDay = currentShowingDateTimeDaysInMonth;
            if (_calendarView.MaxDateRange.Day < currentShowingDateTimeDaysInMonth)
            {
                maxDay = _calendarView.MaxDateRange.Day;
            }
            
            var newMinDateTime = new DateTime(currentShowingYear, currrentShowingMonth, minDay); // 예: 2021년 2월 1일
            var newMaxDateTime = new DateTime(currentShowingYear, currrentShowingMonth, maxDay); // 예: 2021년 2월 28일

            var isCanExcecute = false;
            if (direction.Equals(Direction.Previous))
            {
                if (IsPreviousEnabled(newMinDateTime))
                {
                    isCanExcecute = true;
                }
            }
            else if (direction.Equals(Direction.Next))
            {
                if (IsNextEnabled(newMaxDateTime))
                {
                    isCanExcecute = true;
                }
            }

            return isCanExcecute;
        }

        private bool IsPreviousEnabled(DateTime minDateTime)
        {
            if (CurrentCalendarView.Equals(ViewType.MonthView))
            {
                return minDateTime.AddMonths(-1) >= _calendarView.MinDateRange;
            }
            else if (CurrentCalendarView.Equals(ViewType.YearView))
            {
                return minDateTime.AddYears(-1) >= _calendarView.MinDateRange;
            }
            else
            {
                return false;
            }
        }

        private bool IsNextEnabled(DateTime maxDateTime)
        {
            if (CurrentCalendarView.Equals(ViewType.MonthView))
            {
                return maxDateTime.AddMonths(+1) <= _calendarView.MaxDateRange;
            }
            else if (CurrentCalendarView.Equals(ViewType.YearView))
            {
                return maxDateTime.AddYears(+1) <= _calendarView.MaxDateRange;
            }
            else
            {
                return false;
            }
        }

        private void UpdateCalendarCurrent()
        {
            if (CurrentCalendarView.Equals(ViewType.MonthView))
            {
                _calendarView.YearLayout(_calendarView._currentShowingDateTime);
            }

            else if (CurrentCalendarView.Equals(ViewType.YearView))
            {
                _calendarView.MonthLayout(DateTime.Now);
            }
        }

        private void UpdateCalendarWithDirection(Direction direction)
        {
            if (direction.Equals(Direction.Previous))
            {
                if (CurrentCalendarView.Equals(ViewType.MonthView))
                {
                    _calendarView.MonthView(_calendarView._currentShowingDateTime.AddMonths(-1));
                }
                else if (CurrentCalendarView.Equals(ViewType.YearView))
                {
                    _calendarView.YearView(_calendarView._currentShowingDateTime.AddYears(-1));
                }
            }

            else if (direction.Equals(Direction.Next))
            {
                if (CurrentCalendarView.Equals(ViewType.MonthView))
                {
                    _calendarView.MonthView(_calendarView._currentShowingDateTime.AddMonths(+1));

                }
                else if (CurrentCalendarView.Equals(ViewType.YearView))
                {
                    _calendarView.YearView(_calendarView._currentShowingDateTime.AddYears(+1));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}