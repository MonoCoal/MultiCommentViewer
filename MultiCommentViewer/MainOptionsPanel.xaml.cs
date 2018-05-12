﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MultiCommentViewer.Test;
using SitePlugin;
namespace MultiCommentViewer
{
    /// <summary>
    /// Interaction logic for MainOptionsPanel.xaml
    /// </summary>
    public partial class MainOptionsPanel : UserControl
    {
        public MainOptionsPanel()
        {
            InitializeComponent();
        }
        internal void SetViewModel(MainOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        internal MainOptionsViewModel GetViewModel()
        {
            return (MainOptionsViewModel)this.DataContext;
        }
    }
    public class FontFamilyViewModel
    {
        public string Text { get; private set; }
        public FontFamily FontFamily { get; private set; }

        public FontFamilyViewModel(FontFamily fontFamily, CultureInfo culture)
        {
            Text = ConvertFontFamilyToName(fontFamily, culture);
            FontFamily = fontFamily;
        }
        public override bool Equals(object obj)
        {
            var b = obj as FontFamilyViewModel;
            if (b == null)
                return false;
            return this.FontFamily.Equals(b.FontFamily);
        }
        public override int GetHashCode()
        {
            return FontFamily.GetHashCode();
        }
        public static string ConvertFontFamilyToName(FontFamily fontFamily, CultureInfo culture)
        {
            string text;
            var lang = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
            if (fontFamily.FamilyNames.ContainsKey(lang))
            {
                text = fontFamily.FamilyNames[lang];
            }
            else
            {
                text = fontFamily.ToString();
            }
            return text;
        }
    }
    class MainOptionsViewModel
    {
        public Color BackColor
        {
            get { return ChangedOptions.BackColor; }
            set { ChangedOptions.BackColor = value; }
        }
        public Color ForeColor
        {
            get { return ChangedOptions.ForeColor; }
            set { ChangedOptions.ForeColor = value; }
        }
        public Color NoticeCommentBackColor
        {
            get { return ChangedOptions.InfoBackColor; }
            set { ChangedOptions.InfoBackColor = value; }
        }
        public Color NoticeCommentForeColor
        {
            get { return ChangedOptions.InfoForeColor; }
            set { ChangedOptions.InfoForeColor = value; }
        }
        public Color SelectedRowBackColor
        {
            get { return ChangedOptions.SelectedRowBackColor; }
            set { ChangedOptions.SelectedRowBackColor = value; }
        }
        public Color SelectedRowForeColor
        {
            get { return ChangedOptions.SelectedRowForeColor; }
            set { ChangedOptions.SelectedRowForeColor = value; }
        }
        public Color VerticalGridLineColor
        {
            get { return ChangedOptions.VerticalGridLineColor; }
            set { ChangedOptions.VerticalGridLineColor = value; }
        }
        public Color HorizontalGridLineColor
        {
            get { return ChangedOptions.HorizontalGridLineColor; }
            set { ChangedOptions.HorizontalGridLineColor = value; }
        }
        public bool IsUserNameWrapping
        {
            get { return ChangedOptions.IsUserNameWrapping; }
            set { ChangedOptions.IsUserNameWrapping = value; }
        }
        public bool IsAddingNewCommentTop
        {
            get { return ChangedOptions.IsAddingNewCommentTop; }
            set { ChangedOptions.IsAddingNewCommentTop = value; }
        }
        public bool IsAutoCheckIfUpdateExists
        {
            get { return ChangedOptions.IsAutoCheckIfUpdateExists; }
            set { ChangedOptions.IsAutoCheckIfUpdateExists = value; }
        }
        public FontFamilyViewModel FontFamily
        {
            get { return new FontFamilyViewModel(ChangedOptions.FontFamily, CultureInfo.CurrentCulture); }
            set { ChangedOptions.FontFamily = value.FontFamily; }
        }
        public int FontSize
        {
            get { return ChangedOptions.FontSize; }
            set { ChangedOptions.FontSize = value; }
        }
        public bool IsBold
        {
            get
            {
                return ChangedOptions.FontWeight == FontWeights.Bold;
            }
            set
            {
                var b = value;
                if (b)
                {
                    ChangedOptions.FontWeight = FontWeights.Bold;
                }
                else
                {
                    ChangedOptions.FontWeight = FontWeights.Normal;
                }
            }
        }
        public FontFamilyViewModel FirstCommentFontFamily
        {
            get { return new FontFamilyViewModel(ChangedOptions.FirstCommentFontFamily, CultureInfo.CurrentCulture); }
            set { ChangedOptions.FirstCommentFontFamily = value.FontFamily; }
        }
        public int FirstCommentFontSize
        {
            get { return ChangedOptions.FirstCommentFontSize; }
            set { ChangedOptions.FirstCommentFontSize = value; }
        }
        public bool IsFirstCommentBold
        {
            get
            {
                return ChangedOptions.FirstCommentFontWeight == FontWeights.Bold;
            }
            set
            {
                var b = value;
                if (b)
                {
                    ChangedOptions.FirstCommentFontWeight = FontWeights.Bold;
                }
                else
                {
                    ChangedOptions.FirstCommentFontWeight = FontWeights.Normal;
                }
            }
        }
        public bool IsPixelScrolling
        {
            get { return ChangedOptions.IsPixelScrolling; }
            set { ChangedOptions.IsPixelScrolling = value; }
        }
        private readonly IOptions _origin;
        private readonly IOptions _changed;
        public IOptions OriginOptions { get { return _origin; } }
        public IOptions ChangedOptions { get { return _changed; } }
        public ObservableCollection<FontFamilyViewModel> FontFamillyCollection { get; private set; }
        public ObservableCollection<int> FontSizeCollection { get; private set; }
        public MainOptionsViewModel(IOptions options)
        {
            _origin = options;
            _changed = options.Clone() as IOptions;

            var fontList = Fonts.SystemFontFamilies.OrderBy(f => f.ToString()).Select(f => new FontFamilyViewModel(f, CultureInfo.CurrentCulture));
            FontFamillyCollection = new ObservableCollection<FontFamilyViewModel>(fontList);
            FontFamily = new FontFamilyViewModel(new FontFamily("Meiryo"), CultureInfo.CurrentCulture);

            var sizeList = Enumerable.Range(6, 40);
            FontSizeCollection = new ObservableCollection<int>(sizeList);
            FontSize = 10;
        }
        public MainOptionsViewModel()
        {
            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                _origin = new DynamicOptionsTest
                {
                    ForeColor = Colors.Red,
                    BackColor = Colors.Black,
                    InfoBackColor = Colors.Yellow,
                    InfoForeColor = Colors.Black,
                    SelectedRowBackColor = Colors.Aqua,
                    SelectedRowForeColor = Colors.Pink,
                    VerticalGridLineColor = Colors.Green,
                    HorizontalGridLineColor = Colors.LightGray,
                    FontFamily = new FontFamily("メイリオ"),
                     IsPixelScrolling = false,
                };
                _changed = _origin.Clone() as IOptions;
                IsBold = true;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
    class MainTabPage : IOptionsTabPage
    {
        public string HeaderText { get; }

        public UserControl TabPagePanel => _panel;

        public void Apply()
        {
            var optionsVm = _panel.GetViewModel();
            optionsVm.OriginOptions.Set(optionsVm.ChangedOptions);
        }

        public void Cancel()
        {
        }
        private readonly MainOptionsPanel _panel;
        public MainTabPage(string displayName, MainOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
    internal class FontFamilyToFontFamilyViewModelConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fontFamily = value as FontFamily;
            return new FontFamilyViewModel(fontFamily, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewModel = value as FontFamilyViewModel;
            return viewModel.FontFamily;

        }
    }
}