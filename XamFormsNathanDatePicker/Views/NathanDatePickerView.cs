using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace XamFormsNathanDatePicker
{
    public partial class NathanDatePickerView : ContentView
    {
        public EventHandler<DateTime> DateSelected;

        private readonly Color _selectedDateColor = Color.FromRgba(0, 0, 0, 0.3);
        private readonly Color _disabledColor = Color.LightGray;
        private readonly Color _oldMonthDatesColor = Color.White;
        private readonly Color _activatedColor = Color.White;
        private readonly Color _textColor = Color.Black;

        private const int YearMaxRow = 4;
        private const int YearMaxColumn = 3;

        private readonly string[] WeekDaysLabel = { "일", "월", "화", "수", "목", "금", "토" };
        private readonly string[] MonthsLabel = { "1월", "2월", "3월", "4월", "5월", "6월", "7월", "8월", "9월", "10월", "11월", "12월" };

        public DateTime MinDateRange { get; set; } = DateTime.MinValue;
        public DateTime MaxDateRange { get; set; } = DateTime.MaxValue;

        public DateTime _currentShowingDateTime = DateTime.Now;

        private readonly NathanDatePickerViewModel _calendarViewModel;

        private CustomCalendarCellFrame _selectedCellFrame = null;
        
        private Grid _calendarView;
        private StackLayout _calendarHolder;

        private Grid _monthGrid = null;
        private StackLayout _yearStackLayout = null;

        private List<CustomCalendarCellFrame> _monthStackLayoutList;
        private List<CustomCalendarCellFrame> _yearStackLayoutList;

        private readonly TapGestureRecognizer _tapGestureRecognizer = new TapGestureRecognizer();

        private static readonly BindableProperty SelectedDateProperty = BindableProperty.Create(
            nameof(SelectedDate),
            typeof(DateTime),
            typeof(NathanDatePickerView),
            null,
            propertyChanged: OnSelectedDateChanged);

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }
        private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((NathanDatePickerView) bindable)._selectedDate = (DateTime) newValue;
            ((NathanDatePickerView) bindable).MonthLayout(((NathanDatePickerView) bindable)._currentShowingDateTime);
        }

        public NathanDatePickerView()
        {
            Initialize();

            _calendarViewModel = new NathanDatePickerViewModel(this)
            {
                CurrentDate = $"{_currentShowingDateTime:MMMM}, {_currentShowingDateTime.Year}"
            };
            BindingContext = _calendarViewModel;

            _tapGestureRecognizer.Tapped += CalendarDateSelected;

            MonthLayout(_currentShowingDateTime);
        }

        private void Initialize()
        {
            Padding = 0;
            Margin = 0;

            _calendarView = new Grid
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            var rightGesture = new SwipeGestureRecognizer
            {
                Direction = SwipeDirection.Right
            };
            rightGesture.SetBinding(SwipeGestureRecognizer.CommandProperty, "NextCalendarCommand");
            rightGesture.CommandParameter = Direction.Previous;

            var leftGesture = new SwipeGestureRecognizer
            {
                Direction = SwipeDirection.Left
            };
            leftGesture.SetBinding(SwipeGestureRecognizer.CommandProperty, "PreviousCalendarCommand");
            leftGesture.CommandParameter = Direction.Next;

            _calendarView.GestureRecognizers.Add(rightGesture);
            _calendarView.GestureRecognizers.Add(leftGesture);

            _calendarView.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)});
            _calendarView.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Star)});

            var leftImageButton = new ImageButton
            {
                Padding = 10,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Source = "btn",
                BackgroundColor = Color.Transparent,
                HeightRequest = 40
            };
            leftImageButton.SetBinding(ImageButton.CommandProperty, "PreviousCalendarCommand");
            leftImageButton.CommandParameter = Direction.Previous;

            var currentDateButton = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 180,
                TextColor = Color.Gray,
                FontSize = 14,
                BackgroundColor = Color.Transparent
            };
            currentDateButton.SetBinding(Button.TextProperty, "CurrentDate");
            currentDateButton.SetBinding(Button.CommandProperty, "CurrentCalendarCommand");

            var rightImageButton = new ImageButton
            {
                Padding=10,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Source = "btn",
                BackgroundColor = Color.Transparent,
                HeightRequest = 40
            };
            rightImageButton.SetBinding(ImageButton.CommandProperty, "NextCalendarCommand");
            rightImageButton.CommandParameter = Direction.Next;

            var headerStackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(0, 0, 0, 20),
                Children = {leftImageButton, currentDateButton, rightImageButton}
            };

            _calendarView.Children.Add(headerStackLayout, 0, 0);

            _calendarHolder = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            _calendarView.Children.Add(_calendarHolder, 0, 1);

            Content = _calendarView;
        }

        public void YearLayout(DateTime dateTime)
        {
            if (_monthGrid != null)
            {
                _monthGrid.IsVisible = false;
            }
            if(_yearStackLayout != null)
            {
                _yearStackLayout.IsVisible = true;
                YearView(dateTime);

                return;
            }

            _yearStackLayoutList = new List<CustomCalendarCellFrame>();
            _yearStackLayout = new StackLayout()
            {
                Spacing = 40,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            _calendarHolder.Children.Add(_yearStackLayout);
            for (var row = 0; row < YearMaxRow; row++)
            {
                var yearRowStackLayout = new StackLayout()
                {
                    Orientation = StackOrientation.Horizontal, Spacing = 5,
                    HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand
                };
                _yearStackLayout.Children.Add(yearRowStackLayout);

                for (var col = 0; col < YearMaxColumn; col++)
                {
                    var monthCellFrame = new CustomCalendarCellFrame()
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        BackgroundColor = _activatedColor
                    };
                    var yearLabel = new Label()
                    {
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.CenterAndExpand
                    };
                    _yearStackLayoutList.Add(monthCellFrame);
                    monthCellFrame.Content = yearLabel;

                    yearRowStackLayout.Children.Add(monthCellFrame);
                }
            }
            YearView(dateTime);           
        }

        public void YearView(DateTime dateTime)
        {
            var monthCounter = 0;
            _currentShowingDateTime = dateTime;
            _calendarViewModel.CurrentCalendarView = ViewType.YearView;
            _calendarViewModel.CurrentDate = $"{dateTime.Year}";
            for (var row = 0; row < YearMaxRow; row++)
            {
                for (var col = 0; col < YearMaxColumn; col++)
                {
                    var yearCellFrame = _yearStackLayoutList[monthCounter];

                    if (yearCellFrame.Children[0] is Label yearLabel)
                    {
                        yearLabel.Text = MonthsLabel[monthCounter];
                        yearLabel.TextColor = _textColor;

                        yearCellFrame.DateTimeInfo = new DateTime(_currentShowingDateTime.Year, monthCounter + 1, 1);
                        yearCellFrame.CalendarViewType = ViewType.YearView;
                        yearCellFrame.BackgroundColor = _disabledColor;


                        CheckIfValid(yearCellFrame);

                        if (_selectedDate.Year == dateTime.Year && _selectedDate.Month == monthCounter + 1)
                        {
                            yearCellFrame.BackgroundColor = _selectedDateColor;
                            yearLabel.TextColor = Color.Black;
                        }

                        if (DateTime.Now.Year == dateTime.Year && DateTime.Now.Month == monthCounter + 1)
                        {
                            yearLabel.TextColor = Color.Accent;
                        }
                        else
                        {
                            yearLabel.TextColor = Color.Black;
                        }
                    }
                    monthCounter++;
                }
            }
        }


        public void MonthLayout(DateTime dateTime)
        {
            _calendarViewModel.CurrentCalendarView = ViewType.MonthView;
            if (_yearStackLayout != null)
            {
                _yearStackLayout.IsVisible = false;
            }
            _calendarViewModel.CurrentDate = $"{dateTime:MMMM}, {dateTime.Year}";

            if (_monthGrid == null)
            {
                _monthStackLayoutList = new List<CustomCalendarCellFrame>();
                _monthGrid = new Grid
                {
                    ColumnSpacing = 0,
                    RowSpacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                };
                _calendarHolder.Children.Add(_monthGrid);

                _monthGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                for (var i = 0; i < 6; i++)
                {
                    _monthGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                }

                for (var i = 0; i < 7; i++)
                {
                    _monthGrid.ColumnDefinitions.Add(
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                for (var row = 0; row < 7; row++)
                {
                    for (var col = 0; col < 7; col++)
                    {
                        var cell = new CustomCalendarCellFrame
                        {
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                        };
                        var label = new Label
                        {
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalTextAlignment = TextAlignment.Center,
                            TextColor = _textColor,
                            FontSize = 14
                        };
                        cell.Content = label;
                        _monthStackLayoutList.Add(cell);
                        _monthGrid.Children.Add(cell, col, row);
                        if (row == 0)
                        {
                            label.TextColor = Color.Gray;
                            label.Text = WeekDaysLabel[col];
                        }
                    }
                }
            }
            else
            {
                _monthGrid.IsVisible = true;
            }

            MonthView(dateTime);
        }

        public void MonthView(DateTime dateTime)
        {
            _currentShowingDateTime = dateTime;
            _calendarViewModel.CurrentDate = $"{dateTime:MMMM} {dateTime.Year}";

            var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month); // 지정한 연도, 달의 날짜 수
            var startDay = (int) new DateTime(dateTime.Year, dateTime.Month, 1).DayOfWeek; // 시작 요일 (월요일==1) 

            var previousMonthDateTime = _currentShowingDateTime.AddMonths(-1);
            var previousMonthDaysInMonth = DateTime.DaysInMonth(previousMonthDateTime.Year, previousMonthDateTime.Month);

            var counter = 0;
            var nextMonthCounter = 1;

            for (var row = 0; row < 7; row++)
            {
                for (var col = 0; col < 7; col++)
                {
                    var cellFrame = _monthStackLayoutList[counter + 7]; // 처음 7개는 월화수목금토일 줄.
                    var monthLabel = cellFrame.Content as Label;

                    cellFrame.CalendarViewType = ViewType.MonthView;

                    if (row == 0)
                    {
                        continue; // 월화수목금 줄
                    }

                    #region previous month

                    if (counter < startDay)
                    {
                        var previousMonthDate = previousMonthDaysInMonth - (startDay - counter - 1); // 첫 주에 보이는 전달의 마지막 날짜

                        if (monthLabel != null)
                        {
                            monthLabel.Text = previousMonthDate.ToString();
                            monthLabel.TextColor = Color.LightGray;
                        }
                        cellFrame.BackgroundColor = _oldMonthDatesColor;
                        cellFrame.DateTimeInfo = new DateTime(_currentShowingDateTime.AddMonths(-1).Year, _currentShowingDateTime.AddMonths(-1).Month, previousMonthDate);
                    }

                    #endregion previous month

                    #region current Month

                    else if (counter >= startDay && (counter - startDay) < daysInMonth)
                    {
                        var currentMonthDate = counter + 1 - startDay;

                        if (monthLabel != null)
                        {
                            monthLabel.Text = currentMonthDate.ToString();
                            monthLabel.TextColor = Color.Black;

                            if (_selectedDate.Day == currentMonthDate && _selectedDate.Month == _currentShowingDateTime.Month &&
                                _selectedDate.Year == _currentShowingDateTime.Year)
                            {
                                cellFrame.BackgroundColor = _selectedDateColor;
                            }
                            else
                            {
                                cellFrame.BackgroundColor = Color.White;
                            }

                            if (DateTime.Now.Year == _currentShowingDateTime.Year &&
                                DateTime.Now.Month == _currentShowingDateTime.Month &&
                                DateTime.Now.Day == currentMonthDate)
                            {
                                monthLabel.TextColor = Color.Accent;
                            }
                            else
                            {
                                monthLabel.TextColor = Color.Black;
                            }
                        }
                        cellFrame.IsCurrentMonthsDate = true;
                        cellFrame.DateTimeInfo = new DateTime(_currentShowingDateTime.Year, _currentShowingDateTime.Month, currentMonthDate);
                    }

                    #endregion current Month

                    #region next month

                    else if (counter >= (daysInMonth + startDay))
                    {
                        var nexMonthDate = nextMonthCounter++;
                        if (monthLabel != null)
                        {
                            monthLabel.Text = nexMonthDate.ToString();
                            monthLabel.TextColor = Color.LightGray;
                        }
                        cellFrame.BackgroundColor = _oldMonthDatesColor;
                        cellFrame.DateTimeInfo = new DateTime(_currentShowingDateTime.AddMonths(+1).Year, _currentShowingDateTime.AddMonths(+1).Month, nexMonthDate);
                    }
                    #endregion next month

                    CheckIfValid(cellFrame);

                    counter++;
                }
                
            }
        }


        private void CheckIfValid(CustomCalendarCellFrame cellFrame)
        {
            cellFrame.GestureRecognizers.Clear();

            var currentShowingYear = cellFrame.DateTimeInfo.Year;
            var currrentShowingMonth = cellFrame.DateTimeInfo.Month;
            var currentShowingDateTimeDaysInMonth = DateTime.DaysInMonth(currentShowingYear, currrentShowingMonth);

            int minDay = MinDateRange.Day;
            int maxDay = MaxDateRange.Day;
            if (currentShowingDateTimeDaysInMonth <= MaxDateRange.Day)
            {
                maxDay = currentShowingDateTimeDaysInMonth;
            }

            var newMinDateTime = new DateTime(currentShowingYear, currrentShowingMonth, minDay);
            var newMaxDateTime = new DateTime(currentShowingYear, currrentShowingMonth, maxDay);

            if (cellFrame.CalendarViewType.Equals(ViewType.YearView))
            {
                if (newMinDateTime.Date >= MinDateRange.Date &&
                    newMaxDateTime.Date <= MaxDateRange.Date)
                {
                    cellFrame.GestureRecognizers.Add(_tapGestureRecognizer);
                    cellFrame.BackgroundColor = _activatedColor;
                }
            }
            else if (cellFrame.CalendarViewType.Equals(ViewType.MonthView))
            {
                if (cellFrame.DateTimeInfo.Date >= MinDateRange.Date &&
                    cellFrame.DateTimeInfo.Date <= MaxDateRange.Date)
                {
                    if (cellFrame.DateTimeInfo.Month == _currentShowingDateTime.Month)
                    {
                        cellFrame.GestureRecognizers.Add(_tapGestureRecognizer);
                        cellFrame.IsWeekEndingDate = true;
                    }
                }
            }
        }

        private void CalendarDateSelected(object sender, EventArgs e)
        {
            if (!(sender is CustomCalendarCellFrame selectedCellFrame)) { return; }

            if (_selectedCellFrame != null)
            {
                ClearPreviousSelected(_selectedCellFrame);
            }

            _selectedCellFrame = selectedCellFrame;

            selectedCellFrame.BackgroundColor = _selectedDateColor;

            if (selectedCellFrame.CalendarViewType.Equals(ViewType.MonthView))
            {
                _selectedDate = selectedCellFrame.DateTimeInfo;
                DateSelected?.Invoke(this, _selectedDate);

                BackToCurrentDate();
            }
            else if (selectedCellFrame.CalendarViewType.Equals(ViewType.YearView))
            {
                MonthLayout(selectedCellFrame.DateTimeInfo);
            }
        }

        private void ClearPreviousSelected(CustomCalendarCellFrame cellFrame)
        {
            switch (cellFrame.CalendarViewType)
            {
                case ViewType.YearView:
                case ViewType.MonthView when cellFrame.IsWeekEndingDate:
                    cellFrame.BackgroundColor = _activatedColor;
                    break;
                case ViewType.MonthView when cellFrame.IsCurrentMonthsDate:
                    cellFrame.BackgroundColor = _disabledColor;
                    break;
                case ViewType.MonthView:
                    cellFrame.BackgroundColor = _oldMonthDatesColor;
                    break;
            }
        }

        public void BackToCurrentDate()
        {
            MonthLayout(_currentShowingDateTime = DateTime.Now);
        }
    }
}