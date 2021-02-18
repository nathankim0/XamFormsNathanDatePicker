using System;
using Xamarin.Forms;

namespace XamFormsNathanDatePicker
{
    public class CustomCalendarCellFrame : Frame
    {
        public CustomCalendarCellFrame()
        {
            Padding = 0;
            Margin = 0;
            HasShadow = false;
            HeightRequest = 40; 
            WidthRequest = 40;
            HorizontalOptions = LayoutOptions.Start;
            VerticalOptions = LayoutOptions.Start;
        }

        private bool _currentMonth;
        public bool IsCurrentMonthsDate
        {
            get { return _currentMonth; }
            set
            {
                _currentMonth = value;
            }
        }

        private bool _isWeekEndingDate = false;
        public bool IsWeekEndingDate
        {
            get { return _isWeekEndingDate; }
            set
            {
                _isWeekEndingDate = value;
            }
        }
        private DateTime _dateTime;
        public DateTime DateTimeInfo
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
            }
        }

        private ViewType _calendarViewType;
        public ViewType CalendarViewType
        {
            get { return _calendarViewType; }
            set
            {
                _calendarViewType = value;
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
            }
        }

    }
}
