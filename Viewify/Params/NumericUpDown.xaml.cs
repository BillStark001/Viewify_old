using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Viewify.Params
{
    /// <summary>
    /// NumericUpDown.xaml 's interactive logic
    /// Partially referred from this page: https://www.codeproject.com/Articles/139629/A-Numeric-Up-Down-Control-for-WPF
    /// </summary>
    public partial class NumericUpDown : UserControl
    {


        public NumericUpDown()
        {
            InitializeComponent();
        }

        /// <summary>
        /// </summary>
        public int MaxReservedDigit
        {
            get { return (int)GetValue(MaxReservedDigitProperty); }
            set { SetValue(MaxReservedDigitProperty, value); }
        }
        public static readonly DependencyProperty MaxReservedDigitProperty =
            DependencyProperty.Register(
                "MaxReservedDigit",
                typeof(int),
                typeof(NumericUpDown),
                new UIPropertyMetadata(0)
            );

        /// <summary>
        /// 
        /// </summary>
        public bool RoundDisplay
        {
            get { return (bool)GetValue(RoundDisplayProperty); }
            set { SetValue(RoundDisplayProperty, value); }
        }
        public static readonly DependencyProperty RoundDisplayProperty =
            DependencyProperty.Register(
                "RoundDisplay",
                typeof(bool),
                typeof(NumericUpDown),
                new UIPropertyMetadata(true)
            );

        /// <summary>
        /// 
        /// </summary>
        public bool FillDisplay
        {
            get { return (bool)GetValue(FillDisplayProperty); }
            set { SetValue(FillDisplayProperty, value); }
        }
        public static readonly DependencyProperty FillDisplayProperty =
            DependencyProperty.Register(
                "FillDisplay",
                typeof(bool),
                typeof(NumericUpDown),
                new UIPropertyMetadata(false)
            );



        /// <summary>
        /// Maximum value for the Numeric Up Down control
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                "Maximum",
                typeof(double),
                typeof(NumericUpDown),
                new UIPropertyMetadata(1048576d)
            );

        /// <summary>
        /// Minimum value of the numeric up down control.
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                "Minimum", 
                typeof(double),
                typeof(NumericUpDown), 
                new UIPropertyMetadata(-1048576d)
            );

        public double Value
        {
            get
            {
                return (double)GetValue(ValueProperty);
            }
            set
            {
                ValueInput.Text = value.ReserveDigit(MaxReservedDigit, RoundDisplay, FillDisplay);
                SetValue(ValueProperty, value);
            }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", 
                typeof(double), 
                typeof(NumericUpDown),
                new PropertyMetadata(0d, new PropertyChangedCallback(OnValueChanged))
            );

        public int IntValue
        {
            get
            {
                return (int)Value;
            }
            set
            {
                Value = value;
            }
        }

        public int RoundedIntValue
        {
            get
            {
                return (int)(Value + 0.5);
            }
            set
            {
                Value = value;
            }
        }
        // TODO data bindings of these two?

        private static void OnValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)target;
            control.ValueInput.Text = ((double)e.NewValue).ReserveDigit(control.MaxReservedDigit, control.RoundDisplay, control.FillDisplay);
        }

        private void ValueInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ValueInput_LostFocus(sender, e);
        }

        private void ValueInput_LostFocus(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            if (double.TryParse(tb.Text, out var tmpvar))
            {
                Value = tmpvar;
                if (Value < Minimum) Value = Minimum;
                if (Value > Maximum) Value = Maximum;
            }
            tb.Text = Value.ReserveDigit(MaxReservedDigit, RoundDisplay, FillDisplay);
            RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }

        // Value changed
        private static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(
                "ValueChanged", 
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), 
                typeof(NumericUpDown)
            );

        /// <summary>
        /// The ValueChanged event is called when the
        /// ValueInput of the control changes.
        /// </summary>
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        // increase & decrease

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            if (Value < Maximum)
            {
                Value += 1;
                RaiseEvent(new RoutedEventArgs(IncreaseClickedEvent));
            }
        }

        //Increase button clicked
        private static readonly RoutedEvent IncreaseClickedEvent =
            EventManager.RegisterRoutedEvent(
                "IncreaseClicked", 
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), 
                typeof(NumericUpDown)
            );

        /// <summary>
        /// The IncreaseClicked event is called when the
        /// Increase button clicked
        /// </summary>
        public event RoutedEventHandler IncreaseClicked
        {
            add { AddHandler(IncreaseClickedEvent, value); }
            remove { RemoveHandler(IncreaseClickedEvent, value); }
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            if (Value > Minimum)
            {
                Value -= 1;
                RaiseEvent(new RoutedEventArgs(DecreaseClickedEvent));
            }
        }

        //Decrease button clicked
        private static readonly RoutedEvent DecreaseClickedEvent =
            EventManager.RegisterRoutedEvent(
                "DecreaseClicked",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(NumericUpDown)
            );

        /// <summary>
        /// The DecreaseClicked event is called when the
        /// Decrease button clicked
        /// </summary>
        public event RoutedEventHandler DecreaseClicked
        {
            add { AddHandler(DecreaseClickedEvent, value); }
            remove { RemoveHandler(DecreaseClickedEvent, value); }
        }


        /// <summary>
        /// Checking for Up and Down events and updating the value accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && e.Key == Key.Up && Value < Maximum)
            {
                Value++;
                RaiseEvent(new RoutedEventArgs(IncreaseClickedEvent));
            }
            else if (e.IsDown && e.Key == Key.Down && Value > Minimum)
            {
                Value--;
                RaiseEvent(new RoutedEventArgs(DecreaseClickedEvent));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            var text = tb.Text.Insert(tb.CaretIndex, e.Text);

            e.Handled = !double.TryParse(text, out var _);
        }

    }
}
